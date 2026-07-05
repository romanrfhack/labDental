export interface CatalogImageOption {
  path: string;
  label: string;
  note?: string;
}

const productImage = (fileName: string) => `assets/catalog/products/${fileName}`;

export const CATALOG_IMAGE_OPTIONS: readonly CatalogImageOption[] = [
  {
    path: productImage('zirconia-corona-estratificada.webp'),
    label: 'Zirconia - Corona estratificada'
  },
  {
    path: productImage('zirconia-corona-monolitica.webp'),
    label: 'Zirconia - Corona monolitica'
  },
  {
    path: productImage('emax-corona-estratificada.webp'),
    label: 'E-MAX - Corona estratificada'
  },
  {
    path: productImage('emax-incrustacion.webp'),
    label: 'E-MAX - Incrustacion'
  },
  {
    path: productImage('signum-corona.webp'),
    label: 'SIGNUM - Corona'
  },
  {
    path: productImage('signum-incrustacion.webp'),
    label: 'SIGNUM - Incrustacion'
  },
  {
    path: productImage('metal-porcelana-corona-sing-ivoclar.webp'),
    label: 'Metal-porcelana - Corona Sing Ivoclar'
  },
  {
    path: productImage('metal-porcelana-corona-sing-ivoclar-1.webp'),
    label: 'Metal-porcelana - Corona Sing Ivoclar alternativa'
  },
  {
    path: productImage('metalicos-incrustacion-metalica.webp'),
    label: 'Metalicos - Incrustacion metalica'
  },
  {
    path: productImage('metalicos-corona-total-metal-ceramico.webp'),
    label: 'Metalicos - Corona total metal ceramico'
  },
  {
    path: productImage('provisionales-yacket-acrilico-provisional.webp'),
    label: 'Provisionales - Yacket acrilico provisional',
    note: 'Ruta heredada con nombre yacket; se conserva porque el asset existe.'
  },
  {
    path: productImage('provisionales-yacket-acrilico-termocurable.webp'),
    label: 'Provisionales - Yacket acrilico termocurable',
    note: 'Ruta heredada con nombre yacket; se conserva porque el asset existe.'
  },
  {
    path: productImage('provisionales-guarda-oclusal-acrilico.webp'),
    label: 'Provisionales - Guarda oclusal de acrilico'
  },
  {
    path: productImage('totally-natural-dentadura-total.webp'),
    label: 'Totally Natural - Dentadura total'
  },
  {
    path: productImage('totally-natural-protesis-bilateral.webp'),
    label: 'Totally Natural - Protesis bilateral'
  },
  {
    path: productImage('iflex-protesis-bilateral.webp'),
    label: 'iFlex - Protesis bilateral'
  },
  {
    path: productImage('iflex-protesis-unilateral-1-2.webp'),
    label: 'iFlex - Protesis unilateral 1 a 2'
  },
  {
    path: productImage('prostodoncia-dentadura-total-luciton.webp'),
    label: 'Prostodoncia - Dentadura total Luciton'
  },
  {
    path: productImage('prostodoncia-dentadura-total-kulzer.webp'),
    label: 'Prostodoncia - Dentadura total Kulzer'
  },
  {
    path: productImage('protesis-removible-unidad-metalica..webp'),
    label: 'Protesis removible - Unidad metalica',
    note: 'Ruta heredada con doble punto; se conserva porque el asset existe.'
  },
  {
    path: productImage('protesis-inmediata-provisional.webp'),
    label: 'Protesis inmediata provisional'
  }
];
