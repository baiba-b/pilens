using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using MudBlazor;
using Pilens.Data;
using Pilens.Data.DTO;
using Pilens.Data.State;
using System;
using System.Linq;
using System.Timers;
using static MudBlazor.CategoryTypes;
using Microsoft.JSInterop;

namespace Pilens.Components.Pages
{
    public partial class Pomodoro
    {
        // Taimera objekts produktīvajam laikam
        private System.Timers.Timer? productiveTimer;
        // Pauzes taimera objekts
        private System.Timers.Timer? pauseTimer;
        // Sekundes, kas atlicis aktīvajam taimerim
        private int RemainingSeconds { get; set; } = 0;

        // Norāda, vai taimeris ir sākts
        private bool StartBtnPressed { get; set; } = false;
        // Norāda, vai taimeris ir pauzē
        private bool StopBtnPressed { get; set; } = false;
        private bool IsShortPause { get; set; } = false;
        private bool IsLongPause { get; set; } = false;
        // Kļūdu ziņa UI
        string ErrorMessage = string.Empty;

        // MudForm validācijas statuss - parāda vai vērtības ir korektas
        bool success;
        // Forma no UI
        private MudForm? pomodoroForm;
        // Lai nepieļautu dubultu inicializāciju, kas var rasties ar taimera objektu 
        private bool isInitialized = false;

        // Pašreizējā sesijas kārtas numurs
        private int CurrSession { get; set; } = 0;
        // Obligātā lauka kļūdas teksts
        string PomodoroReqError = "Šis ir obligāts atribūts!";

        // DTO ar lietotāja konfigurāciju
        PomodoroDTO pomodoroData = new();
        //Lock objekts lai taimeris netruprina atjaunoties kamēr iestata pauzi
        private readonly object _timerLock = new();

        [Inject]
        private IDbContextFactory<ApplicationDbContext> DbContextFactory { get; set; } = default!;

        [Inject]
        private PomodoroState PomodoroState { get; set; } = default!;

        [Inject]
        private IJSRuntime JSRuntime { get; set; } = default!;

        /// <summary>
        /// Formatē atlikušās sekundes cilvēklasāmā formātā mm:ss.
        /// </summary>
        private string DisplayTime =>
            TimeSpan.FromSeconds(RemainingSeconds).ToString(@"mm\:ss");

        /// <summary>
        /// Pomodoro iestatījumu validācija (nepieļauj negatīvas vai nulles vērtības).
        /// </summary>
        private string PomodoroValidation(int arg)
        {
            string errorMessage = string.Empty;

            if (arg < 1)
            {
                errorMessage = "Lūdzu ievadi pozitīvu skaitli.";
                return errorMessage;
            }
            else return errorMessage;
        }

        /// <summary>
        /// Ielādē lietotāja saglabātos Pomodoro datus un izveido PomodoroDTO objektu, lai sadalītu datubāzes un loģikas slāni.
        /// </summary>
        protected override async Task OnInitializedAsync()
        {
            success = true;
            var userId = await getUserId();
            if (userId == null)
            {
                return;
            }

            try
            {
                await using var db = await DbContextFactory.CreateDbContextAsync();
                var existingPomodoro = await db.Pomodoros
               .AsNoTracking()
               .FirstOrDefaultAsync(p => p.UserID == userId);
                if (existingPomodoro is not null)
                {
                    // Izveido DTO no saglabātajiem datiem, lai aizpildītu formu ar lietotāja konfigurāciju
                    pomodoroData = new PomodoroDTO(existingPomodoro);
                    pomodoroData.UserID = userId;
                    RemainingSeconds = pomodoroData.Minutes * 60;
                    PomodoroState.Minutes = pomodoroData.Minutes;
                }
                else
                {
                    // Ja lietotājam nav ieraksta, izmanto DTO noklusējuma vērtības un sinhronizē stāvokli
                    PomodoroState.Minutes = pomodoroData.Minutes;
                }
            }
            catch (Exception)
            {
                string errorMessage = "Kļūda ielādējot datus.";
                SnackbarService.Add(errorMessage, Severity.Error);
                return;
            }

        }

        /// <summary>
        /// Funkcija, kas izveido un sāk taimeri produktīvajai sesijai.
        /// </summary>
        private async Task SetTimer() //System.Timer funkciju implementācijas piemērs & apraksts ņemts no https://learn.microsoft.com/en-us/dotnet/api/system.timers.timer?view=net-9.0  un https://learn.microsoft.com/en-us/aspnet/core/blazor/components/synchronization-context?view=aspnetcore-9.0 
        {
            // Pomodoro formas validācija, lai nevarētu sākt taimeri ar nepieļaujamiem datiem
            if (isInitialized == false)
            {
                await pomodoroForm.Validate();
                if (pomodoroForm.IsValid == false)
                {
                    SnackbarService.Add("Lūdzu, ievadiet korektus datus pirms saglabāšanas.", Severity.Error);
                    return;
                }
            }

            PomodoroState.Minutes = pomodoroData.Minutes;

            string errorMessage = string.Empty;
            // Nedrīkst startēt taimeri, ja eksistē jau kāds taimeris
            if (productiveTimer != null)
            {
                errorMessage = "Kļūda izveidojot taimeri!";
                SnackbarService.Add(errorMessage, Severity.Error);
                productiveTimer = null;
            }
            RemainingSeconds = pomodoroData.Minutes * 60;

            // Izveido taimeri ar sekundes intervālu

            productiveTimer = new System.Timers.Timer(1000);
            // Pievieno taimerim Elapsed notikuma apstrādātāju. (katru sekundi uzsauksi pievienoto funkciju)
            productiveTimer.Elapsed += UpdateTimer;
            productiveTimer.AutoReset = true;
            productiveTimer.Enabled = true;
            StartBtnPressed = true;
            isInitialized = true;
        }


        /// <summary>
        /// Atjauno atlikušās sekundes produktīvajam taimerim un pārslēdz uz pauzēm, kad tas beidzas.
        /// </summary>
        private void UpdateTimer(object source, ElapsedEventArgs e)
        {
            lock (_timerLock)
            {
                if (RemainingSeconds > 0)
                {
                    RemainingSeconds--;
                    InvokeAsync(StateHasChanged);
                }
                else
                {
                    // Kad produktīvais taimeris beidzas
                    if (productiveTimer == null)
                    {
                        string errorMessage = "Kļūda atrodot taimeri!";
                        SnackbarService.Add(errorMessage, Severity.Error);
                        return;
                    }
                    productiveTimer.Stop();
                    productiveTimer.Dispose();
                    productiveTimer = null;
                    CurrSession++;

                    _ = InvokeAsync(() => PlaySoundAsync("yippe"));  // '_' tiek izmantots kā discard, lai ignorētu atgriezto Task un vienkārši palaistu skaņu asinhroni

                    // Izvēlas īso vai garo pauzi
                    if (CurrSession < pomodoroData.SessionAmount && CurrSession % pomodoroData.SessionLongPause != 0)
                    {
                        IsShortPause = true;
                        SetPause(pomodoroData.PauseMinutes);
                    }
                    else if (CurrSession < pomodoroData.SessionAmount)
                    {
                        IsLongPause = true;
                        SetPause(pomodoroData.LongPauseMinutes);
                    }
                    else
                    {   //Restartē visu pēc visu sesiju pabeigšanas
                        isInitialized = false;
                        RemainingSeconds = 0;
                        StartBtnPressed = false;
                        StopBtnPressed = false;
                        CurrSession = 0;
                        InvokeAsync(StateHasChanged);
                    }

                }
            }
        }

        /// <summary>
        /// Startē pauzes taimeri ar dotajām minūtēm.
        /// </summary>
        private void SetPause(int min) //koda implementācijas piemērs ņemts no https://learn.microsoft.com/en-us/dotnet/api/system.timers.timer?view=net-9.0  un https://learn.microsoft.com/en-us/aspnet/core/blazor/components/synchronization-context?view=aspnetcore-9.0 
        {
            RemainingSeconds = min * 60;
            pauseTimer = new System.Timers.Timer(1000);
            pauseTimer.Elapsed += UpdatePauseTimer;
            pauseTimer.AutoReset = true;
            pauseTimer.Enabled = true;
        }



        /// <summary>
        /// Atjauno atlikušās sekundes pauzes laikā un atgriež pie darba taimera, kad pauze beidzas.
        /// </summary>
        private void UpdatePauseTimer(object source, ElapsedEventArgs e)
        {
            lock (_timerLock)
            {
                if (RemainingSeconds > 0)
                {
                    RemainingSeconds--;
                    InvokeAsync(StateHasChanged);
                }
                else
                {
                    // Kad pauze beidzas, atgriežamies pie darba sesijas
                    if (pauseTimer == null)
                    {
                        string errorMessage = "Kļūda atrodot taimeri!";
                        SnackbarService.Add(errorMessage, Severity.Error);
                        return;
                    }
                    pauseTimer?.Stop();
                    pauseTimer?.Dispose();
                    pauseTimer = null;

                    _ = InvokeAsync(() => PlaySoundAsync("item-pick-up"));  // '_' tiek izmantots kā discard, lai ignorētu atgriezto Task un vienkārši palaistu skaņu asinhroni

                    DeactivateActivePause();
                    SetTimer();
                }
            }
        }

        /// <summary>
        /// Aptur jebkuru aktīvo taimeri, notīra pēdējo esošo stāvokli un atiestata sesiju skaitītāju.
        /// </summary>
        private void StopPomodoroTimer()
        {
            // Atkarībā no fāzes aptur pareizo taimeri
            if (!IsShortPause && !IsLongPause)
            {
                if (productiveTimer == null)
                {
                    ErrorMessage = "Kļūda atrodot taimeri!";
                    SnackbarService.Add(ErrorMessage, Severity.Error);
                    return;
                }
                productiveTimer.Stop();
                productiveTimer.Dispose();
                productiveTimer = null;
                isInitialized = false;
            }
            else
            {
                if (pauseTimer == null)
                {
                    ErrorMessage = "Kļūda atrodot taimeri!";
                    SnackbarService.Add(ErrorMessage, Severity.Error);
                    return;
                }
                pauseTimer.Stop();
                pauseTimer.Dispose();
                isInitialized = false;
                pauseTimer = null;
            }

            DeactivateActivePause();

            RemainingSeconds = 0;
            StartBtnPressed = false;
            StopBtnPressed = false;
            CurrSession = 0;
            InvokeAsync(StateHasChanged);
        }

        /// <summary>
        /// Izlaiž aktīvo pauzi un uzreiz startē nākamo darba sesiju.
        /// </summary>
        private void SkipPause(Microsoft.AspNetCore.Components.Web.MouseEventArgs args)
        {
            if (pauseTimer == null)
            {
                string errorMessage = "Kļūda atrodot taimeri!";
                SnackbarService.Add(errorMessage, Severity.Error);
                return;
            }
            pauseTimer.Stop();
            pauseTimer.Dispose();
            pauseTimer = null;
            DeactivateActivePause();
            _ = PlaySoundAsync("item-pick-up");
            SetTimer();
        }

        /// <summary>
        /// Atskaņo skaņu pēc klipa nosaukuma, izmantojot JS interopu. Kods ģenerēts ar AI rīku.
        /// </summary>
        private Task PlaySoundAsync(string clipName)
        {
            if (string.IsNullOrWhiteSpace(clipName))
            {
                return Task.CompletedTask;
            }

            return JSRuntime.InvokeVoidAsync("audioController.playSound", clipName).AsTask();
        }

        /// <summary>
        /// Notīra īsās vai garās pauzes patiesumvērtības.
        /// </summary>
        private void DeactivateActivePause()
        {
            if (IsShortPause)
            {
                IsShortPause = false;
                return;
            }

            if (IsLongPause)
            {
                IsLongPause = false;
            }
        }

        /// <summary>
        /// Aptur aktīvo taimeri (darba vai pauzes) un atzīmē, ka taimeris ir pauzē.
        /// </summary>
        private void PauseTimer(Microsoft.AspNetCore.Components.Web.MouseEventArgs args)
        {

            // Apstādina pareizo taimeri atkarībā no fāzes
            if (!IsShortPause && !IsLongPause)
            {
                if (productiveTimer == null)
                {
                    string errorMessage = "Kļūda atrodot taimeri!";
                    SnackbarService.Add(errorMessage, Severity.Error);
                    return;
                }
                productiveTimer.Stop();
            }
            else
            {
                if (pauseTimer == null)
                {
                    string errorMessage = "Kļūda atrodot taimeri!";
                    SnackbarService.Add(errorMessage, Severity.Error);
                    return;
                }
                pauseTimer.Stop();
            }
            StopBtnPressed = true;
        }

        /// <summary>
        /// Turpina aktīvo taimeri pēc pauzes.
        /// </summary>
        private void ContinueTimer(Microsoft.AspNetCore.Components.Web.MouseEventArgs args)
        {
            // Turpina atbilstošo taimeri
            if (!IsShortPause && !IsLongPause)
            {
                if (productiveTimer == null)
                {
                    string errorMessage = "Kļūda atrodot taimeri!";
                    SnackbarService.Add(errorMessage, Severity.Error);
                    return;
                }
                productiveTimer.Start();
            }
            else
            {
                if (pauseTimer == null)
                {
                    string errorMessage = "Kļūda atrodot taimeri!";
                    SnackbarService.Add(errorMessage, Severity.Error);
                    return;
                }
                pauseTimer.Start();
            }
            StopBtnPressed = false;
            InvokeAsync(StateHasChanged);
        }

        /// <summary>
        /// Manuāli pielāgo atlikušās minūtes (pieskaita vai atņem).
        /// </summary>
        private void AdjustTime(bool adjustType)
        {
            if (adjustType == true)
            {
                RemainingSeconds += pomodoroData.AdjustedMin * 60;
            }
            else
            {
                if (RemainingSeconds > pomodoroData.AdjustedMin * 60) RemainingSeconds -= pomodoroData.AdjustedMin * 60;
                else RemainingSeconds = 0;
            }
        }

        /// <summary>
        /// Nolasa lietotāja Id no autentikācijas konteksta.
        /// </summary>
        async Task<string> getUserId()
        {
            try
            {
                var user = (await _authenticationStateProvider.GetAuthenticationStateAsync()).User;
                var UserId = user.FindFirst(u => u.Type.Contains("nameidentifier"))?.Value;

                return UserId;
            }
            catch (Exception)
            {
                string errorMessage = "Neizdevās identificēt lietotāju.";
                SnackbarService.Add(errorMessage, Severity.Error);
                return string.Empty;
            }
        }
        /// <summary>
        /// Palielina vai inicializē sesiju skaitu atkarībā no taimera statusa.  (Izmanto, lai sāktu vai pievienotu sesijas  Pomodoro taimerim no uzdevumu skata)
        /// </summary>
        public void AddSessions(int sessions)
        {
            if (sessions <= 0)
            {
                return;
            }

            if (StartBtnPressed)
            {
                pomodoroData.SessionAmount += sessions;
                InvokeAsync(StateHasChanged);
            }
            else
            {
                InitializeAndStartSessions(sessions);
            }
        }
        /// <summary>
        /// Inicializē sesiju skaitu un uzreiz startē taimeri (Izmanto, lai sāktu Pomodoro taimeri no uzdevumiem)
        /// </summary>
        public void InitializeAndStartSessions(int sessions)
        {
            pomodoroData.SessionAmount = sessions;
            CurrSession = 0;
            StartBtnPressed = true;
            SetTimer();
        }

        /// <summary>
        /// Saglabā lietotāja Pomodoro taimera konfigurāciju datubāzē un atjauno PomodoroState minūtes, lai tās saņemtu uzdevumu modulis.
        /// </summary>
        private async Task SavePomodoroData(Microsoft.AspNetCore.Components.Web.MouseEventArgs args)
        {
            await pomodoroForm.Validate();
            if (pomodoroForm.IsValid == false)
            {
                SnackbarService.Add("Lūdzu, ievadiet korektus datus pirms saglabāšanas.", Severity.Error);
                return;
            }
            var userId = await getUserId();
            if (userId == null)
            {
                string errorMessage = "Neizdevās identificēt lietotāju.";
                SnackbarService.Add(errorMessage, Severity.Error);
                return;
            }
            pomodoroData.UserID = userId;
            PomodoroState.Minutes = pomodoroData.Minutes;

            try
            {
                await using var db = await DbContextFactory.CreateDbContextAsync();
                var existingPomodoro = await db.Pomodoros
                    .FirstOrDefaultAsync(p => p.UserID == userId);

                if (existingPomodoro is not null)
                {
                    existingPomodoro.Minutes = pomodoroData.Minutes;
                    existingPomodoro.PauseMinutes = pomodoroData.PauseMinutes;
                    existingPomodoro.LongPauseMinutes = pomodoroData.LongPauseMinutes;
                    existingPomodoro.SessionAmount = pomodoroData.SessionAmount;
                    existingPomodoro.SessionLongPause = pomodoroData.SessionLongPause;
                    existingPomodoro.AdjustedMin = pomodoroData.AdjustedMin;
                    db.Pomodoros.Update(existingPomodoro);
                    pomodoroData.updatePomodoro(existingPomodoro, pomodoroData);
                }
                else
                {
                    db.Pomodoros.Add(new Data.Models.Pomodoro
                    {
                        UserID = userId,
                        Minutes = pomodoroData.Minutes,
                        PauseMinutes = pomodoroData.PauseMinutes,
                        LongPauseMinutes = pomodoroData.LongPauseMinutes,
                        SessionAmount = pomodoroData.SessionAmount,
                        SessionLongPause = pomodoroData.SessionLongPause,
                        AdjustedMin = pomodoroData.AdjustedMin
                    });
                }

                await db.SaveChangesAsync();
                SnackbarService.Add("Dati saglabāti veiksmīgi!", Severity.Success);
            }
            catch (Exception)
            {
                SnackbarService.Add("Kļūda saglabājot datus!", Severity.Error);
            }
        }
    }
}
