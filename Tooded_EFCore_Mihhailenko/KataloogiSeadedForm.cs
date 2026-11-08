using Microsoft.EntityFrameworkCore;

namespace Tooded_EFCore;

public sealed class KataloogiSeadedForm : Form
{
    private readonly TextBox url = new() { MaxLength = 2048, PlaceholderText = "https://minu-pood.ee/catalog.json" };
    private readonly Label olek = Vaade.Tekstirida("Laadin kataloogi seadeid…");
    private readonly Label viimane = Vaade.Tekstirida("");
    private readonly Button salvesta = new() { Text = "Salvesta aadress" };
    private readonly Button uuenda = new() { Text = "Uuenda kataloogi" };
    private readonly Button taasta = new() { Text = "Taasta veebikataloogi andmed" };
    private readonly CancellationTokenSource sulgemine = new();
    private readonly KataloogiTeenus teenus = new();

    public KataloogiSeadedForm()
    {
        if (!AktiivneKasutaja.OnAdmin) throw new UnauthorizedAccessException();
        Vaade.Vorm(this, "Veebikataloog", 820, 600);
        MinimumSize = new Size(620, 460);
        var raam = Vaade.Raam(this, "Veebikataloog", "Üks JSON-fail sinu veebiserveris. Ostlemine jätkub ka ilma internetita.");
        var sisu = Vaade.Veerg();
        Vaade.Vali(sisu, "Kataloogi HTTPS-aadress", url);
        var riba = Vaade.Nupuriba();
        foreach (var nupp in new[] { salvesta, uuenda, taasta })
        {
            Vaade.Nupp(nupp, nupp == uuenda);
            riba.Controls.Add(nupp);
        }
        Vaade.Lisa(sisu, riba);
        Vaade.Lisa(sisu, viimane);
        Vaade.Lisa(sisu, olek);
        Vaade.Lisa(sisu, Vaade.Tekstirida("Automaatne kontroll toimub poe avamisel, kõige sagedamini iga 6 tunni järel.\n"
            + "Käsitsi uuendamine küsib serverilt alati viimast versiooni.\n\n"
            + "Veebist imporditud kategooriaid haldab JSON-fail. Kohalikud tooted säilivad.\n"
            + "Imporditud toote kohalik muutmine kaitseb selle andmeid järgmise impordi eest.\n"
            + "Kataloogist eemaldatud toode peidetakse alati; varasemad tellimused säilivad.\n"
            + "Taastamine asendab kõik imporditud toodete kohalikud muudatused veebiversiooniga."));
        raam.Controls.Add(Vaade.Kerimine(Vaade.Kaart(sisu)), 0, 1);
        salvesta.Click += async (_, _) => await TegevusAsync(async () =>
        {
            await teenus.SalvestaUrlAsync(url.Text, sulgemine.Token);
            olek.Text = "Aadress on salvestatud. Kataloogi laadimiseks vajuta „Uuenda kataloogi”.";
        });
        uuenda.Click += async (_, _) => await TegevusAsync(async () =>
        {
            await teenus.SalvestaUrlAsync(url.Text, sulgemine.Token);
            olek.Text = (await teenus.UuendaAsync(true, sulgemine.Token)).Teade;
        });
        taasta.Click += async (_, _) =>
        {
            if (Vaade.Teade("Kas asendan imporditud toodete kohalikud muudatused veebikataloogi andmetega? Kohalikke tooteid ei muudeta.",
                "Veebiandmete taastamine", MessageBoxButtons.YesNo) != DialogResult.Yes) return;
            await TegevusAsync(async () =>
            {
                await teenus.SalvestaUrlAsync(url.Text, sulgemine.Token);
                olek.Text = (await teenus.TaastaKohalikudAsync(sulgemine.Token)).Teade;
            });
        };
        Shown += async (_, _) => await TegevusAsync(async () =>
        {
            await using var db = new ToodedContext();
            var seaded = await db.KataloogiSeaded.AsNoTracking().SingleOrDefaultAsync(sulgemine.Token);
            url.Text = seaded?.Url ?? "";
            olek.Text = "Valmis. HTTPS-aadressi tühjendamine lülitab automaatse uuendamise välja.";
        });
        ResumeLayout(true);
    }

    private async Task TegevusAsync(Func<Task> tegevus)
    {
        salvesta.Enabled = uuenda.Enabled = taasta.Enabled = url.Enabled = false;
        olek.Text = "Laadin… Kohalik kataloog jääb kasutatavaks.";
        try
        {
            await tegevus();
            await using var db = new ToodedContext();
            var seaded = await db.KataloogiSeaded.AsNoTracking().SingleOrDefaultAsync(sulgemine.Token);
            if (!IsDisposed)
                viimane.Text = seaded?.ViimaneEdu == null ? "Kataloogi pole veel imporditud."
                    : $"Viimane edukas import: {seaded.ViimaneEdu.Value.ToLocalTime():dd.MM.yyyy HH:mm} · versioon {seaded.Versioon}\n"
                    + $"Viimane kontroll: {seaded.ViimaneKontroll?.ToLocalTime():dd.MM.yyyy HH:mm}";
        }
        catch (OperationCanceledException) when (sulgemine.IsCancellationRequested) { }
        catch (Exception ex)
        {
            if (!IsDisposed) olek.Text = "Uuendamine ei õnnestunud. Senine kohalik kataloog säilib.\n" + ex.Message;
        }
        finally
        {
            if (!IsDisposed) salvesta.Enabled = uuenda.Enabled = taasta.Enabled = url.Enabled = true;
        }
    }

    protected override void OnFormClosed(FormClosedEventArgs e)
    {
        sulgemine.Cancel();
        base.OnFormClosed(e);
    }
}
