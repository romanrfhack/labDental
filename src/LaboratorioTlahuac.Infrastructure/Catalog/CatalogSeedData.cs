namespace LaboratorioTlahuac.Infrastructure.Catalog;

internal static class CatalogSeedData
{
    public static IReadOnlyCollection<CatalogSectionSeed> Sections { get; } =
    [
        new(
            "zirconia",
            "Zirconia",
            Image("zirconia-corona-estratificada.webp"),
            [
                new("zirconia-corona-estratificada", "Corona estratificada", 1800m, Image("zirconia-corona-estratificada.webp")),
                new("zirconia-corona-monolitica", "Corona monolítica", 1600m, Image("zirconia-corona-monolitica.webp")),
                new("zirconia-carilla", "Carilla", 1600m, null),
                new("zirconia-incrustacion", "Incrustación", 1600m, null)
            ]),
        new(
            "emax",
            "E-MAX",
            Image("emax-corona-estratificada.webp"),
            [
                new("emax-corona-estratificada", "Corona estratificada", 1600m, Image("emax-corona-estratificada.webp")),
                new("emax-carilla", "Carilla", 1500m, null),
                new("emax-incrustacion", "Incrustación", 1500m, Image("emax-incrustacion.webp"))
            ]),
        new(
            "signum",
            "SIGNUM",
            Image("signum-corona.webp"),
            [
                new("signum-corona", "Corona", 1100m, Image("signum-corona.webp")),
                new("signum-carilla", "Carilla", 950m, null),
                new("signum-unidad-puente-malla", "Unidad de puente con malla", 1300m, null),
                new("signum-incrustacion", "Incrustación", 850m, Image("signum-incrustacion.webp"))
            ]),
        new(
            "metal-porcelana",
            "Metal-porcelana",
            Image("metal-porcelana-corona-sing-ivoclar.webp"),
            [
                new("metal-porcelana-corona-sing-ivoclar", "Corona d. Sing Ivoclar", 1350m, Image("metal-porcelana-corona-sing-ivoclar.webp")),
                new("metal-porcelana-corona-safir-kulzer", "Corona Safir Kulzer", 1250m, null)
            ]),
        new(
            "metalicos-auxiliares",
            "Metálicos y auxiliares",
            Image("metalicos-incrustacion-metalica.webp"),
            [
                new("metalicos-incrustacion-metalica", "Incrustación metálica", 750m, Image("metalicos-incrustacion-metalica.webp")),
                new("metalicos-corona-total-metal-ceramico", "Corona total metal cerámico", 800m, Image("metalicos-corona-total-metal-ceramico.webp")),
                new("metalicos-acetato-rigido", "Acetato rígido", 230m, null),
                new("metalicos-acetato-flexible", "Acetato flexible", 280m, null)
            ]),
        new(
            "provisionales-guardas",
            "Provisionales y guardas",
            Image("provisionales-guarda-oclusal-acrilico.webp"),
            [
                new("provisionales-jacket-acrilico-provisional", "Jacket acrílico provisional", 280m, Image("provisionales-yacket-acrilico-provisional.webp")),
                new("provisionales-jacket-acrilico-termocurable", "Jacket acrílico termocurable", 500m, Image("provisionales-yacket-acrilico-termocurable.webp")),
                new("provisionales-guarda-oclusal-acrilico", "Guarda oclusal de acrílico", 1200m, Image("provisionales-guarda-oclusal-acrilico.webp"))
            ]),
        new(
            "totally-natural",
            "Totally Natural by tcs",
            Image("totally-natural-dentadura-total.webp"),
            [
                new("totally-natural-dentadura-total", "Dentadura total c/u", 3200m, Image("totally-natural-dentadura-total.webp")),
                new("totally-natural-protesis-bilateral", "Prótesis bilateral", 2900m, Image("totally-natural-protesis-bilateral.webp")),
                new("totally-natural-protesis-unilateral-1-2", "Prótesis unilateral de 1 a 2 unidades", 1500m, null),
                new("totally-natural-protesis-unilateral-3", "Prótesis unilateral 3 unidades", 1700m, null)
            ]),
        new(
            "iflex",
            "iFlex by tcs",
            Image("iflex-protesis-bilateral.webp"),
            [
                new("iflex-protesis-bilateral", "Prótesis bilateral", 2900m, Image("iflex-protesis-bilateral.webp")),
                new("iflex-protesis-unilateral-1-2", "Prótesis unilateral de 1 a 2 unidades", 1500m, Image("iflex-protesis-unilateral-1-2.webp")),
                new("iflex-protesis-unilateral-3", "Prótesis unilateral 3 unidades", 1700m, null)
            ]),
        new(
            "prostodoncia-parcial-total",
            "Prostodoncia parcial y total",
            Image("prostodoncia-dentadura-total-luciton.webp"),
            [
                new("prostodoncia-dentadura-total-luciton", "Dentadura total acrílico Luciton 199 c/u", 2900m, Image("prostodoncia-dentadura-total-luciton.webp")),
                new("prostodoncia-dentadura-total-kulzer", "Dentadura total en acrílico Kulzer c/u", 2700m, Image("prostodoncia-dentadura-total-kulzer.webp"))
            ]),
        new(
            "servicios-prostodonticos",
            "Servicios prostodónticos",
            null,
            [
                new("servicios-reparacion-dentadura-fractura", "Reparación de dentadura por fractura", 650m, null),
                new("servicios-gancho-volado", "Gancho volado", 300m, null),
                new("servicios-descanso-metalico", "Descanso metálico c/u", 250m, null),
                new("servicios-rebase", "Rebase", 1100m, null),
                new("servicios-aumentar-dientes", "Aumentar dientes c/u", 350m, null)
            ]),
        new(
            "protesis-removible-metal-acrilico",
            "Prótesis removible metal-acrílico",
            Image("protesis-removible-unidad-metalica..webp"),
            [
                new("protesis-removible-unidad-acrilica", "Unidad acrílica", 180m, null),
                new("protesis-removible-unidad-metalica", "Unidad metálica", 240m, Image("protesis-removible-unidad-metalica..webp"))
            ]),
        new(
            "protesis-inmediata-provisional",
            "Prótesis inmediata provisional",
            Image("protesis-inmediata-provisional.webp"),
            [
                new("protesis-inmediata-1-unidad", "Prótesis de 1 unidad", 500m, null),
                new("protesis-inmediata-1-4-unidades", "Prótesis de 1 a 4 unidades", 900m, null),
                new("protesis-inmediata-1-9-unidades", "Prótesis de 1 a 9 unidades", 1300m, null),
                new("protesis-inmediata-10-unidades", "A partir de 10 unidades", 1450m, null)
            ])
    ];

    private static string Image(string fileName)
    {
        return $"assets/catalog/products/{fileName}";
    }
}

internal sealed record CatalogSectionSeed(
    string Key,
    string Name,
    string? ImagePath,
    IReadOnlyCollection<CatalogProductSeed> Products);

internal sealed record CatalogProductSeed(
    string Key,
    string Name,
    decimal PriceAmount,
    string? ImagePath);
