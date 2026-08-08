export interface CatalogProduct {
  id: string;
  name: string;
  description?: string;
  price: number;
  imageUrl?: string;
  altText?: string;
}

export interface CatalogSection {
  id: string;
  name: string;
  description?: string;
  imageUrl?: string;
  altText?: string;
  products: CatalogProduct[];
}

const productImage = (fileName: string) => `assets/catalog/products/${fileName}`;

export const catalogSections: CatalogSection[] = [
  {
    id: 'zirconia',
    name: 'Zirconia',
    imageUrl: productImage('zirconia-corona-estratificada.webp'),
    products: [
      {
        id: 'zirconia-corona-estratificada',
        name: 'Corona estratificada',
        price: 1800,
        imageUrl: productImage('zirconia-corona-estratificada.webp')
      },
      {
        id: 'zirconia-corona-monolitica',
        name: 'Corona monolítica',
        price: 1600,
        imageUrl: productImage('zirconia-corona-monolitica.webp')
      },
      { id: 'zirconia-carilla', name: 'Carilla', price: 1600 },
      { id: 'zirconia-incrustacion', name: 'Incrustación', price: 1600 }
    ]
  },
  {
    id: 'emax',
    name: 'E-MAX',
    imageUrl: productImage('emax-corona-estratificada.webp'),
    products: [
      {
        id: 'emax-corona-estratificada',
        name: 'Corona estratificada',
        price: 1600,
        imageUrl: productImage('emax-corona-estratificada.webp')
      },
      { id: 'emax-carilla', name: 'Carilla', price: 1500 },
      {
        id: 'emax-incrustacion',
        name: 'Incrustación',
        price: 1500,
        imageUrl: productImage('emax-incrustacion.webp')
      }
    ]
  },
  {
    id: 'signum',
    name: 'SIGNUM',
    imageUrl: productImage('signum-corona.webp'),
    products: [
      {
        id: 'signum-corona',
        name: 'Corona',
        price: 1100,
        imageUrl: productImage('signum-corona.webp')
      },
      { id: 'signum-carilla', name: 'Carilla', price: 950 },
      { id: 'signum-unidad-puente-malla', name: 'Unidad de puente con malla', price: 1300 },
      {
        id: 'signum-incrustacion',
        name: 'Incrustación',
        price: 850,
        imageUrl: productImage('signum-incrustacion.webp')
      }
    ]
  },
  {
    id: 'metal-porcelana',
    name: 'Metal-porcelana',
    imageUrl: productImage('metal-porcelana-corona-sing-ivoclar.webp'),
    products: [
      {
        id: 'metal-porcelana-corona-sing-ivoclar',
        name: 'Corona d. Sing Ivoclar',
        price: 1350,
        imageUrl: productImage('metal-porcelana-corona-sing-ivoclar.webp')
      },
      { id: 'metal-porcelana-corona-safir-kulzer', name: 'Corona Safir Kulzer', price: 1250 }
    ]
  },
  {
    id: 'metalicos-auxiliares',
    name: 'Metálicos y auxiliares',
    imageUrl: productImage('metalicos-incrustacion-metalica.webp'),
    products: [
      {
        id: 'metalicos-incrustacion-metalica',
        name: 'Incrustación metálica',
        price: 750,
        imageUrl: productImage('metalicos-incrustacion-metalica.webp')
      },
      {
        id: 'metalicos-corona-total-metal-ceramico',
        name: 'Corona total metal cerámico',
        price: 800,
        imageUrl: productImage('metalicos-corona-total-metal-ceramico.webp')
      },
      { id: 'metalicos-acetato-rigido', name: 'Acetato rígido', price: 230 },
      { id: 'metalicos-acetato-flexible', name: 'Acetato flexible', price: 280 }
    ]
  },
  {
    id: 'provisionales-guardas',
    name: 'Provisionales y guardas',
    imageUrl: productImage('provisionales-guarda-oclusal-acrilico.webp'),
    products: [
      {
        id: 'provisionales-jacket-acrilico-provisional',
        name: 'Jacket acrílico provisional',
        price: 280,
        imageUrl: productImage('provisionales-yacket-acrilico-provisional.webp')
      },
      {
        id: 'provisionales-jacket-acrilico-termocurable',
        name: 'Jacket acrílico termocurable',
        price: 500,
        imageUrl: productImage('provisionales-yacket-acrilico-termocurable.webp')
      },
      {
        id: 'provisionales-guarda-oclusal-acrilico',
        name: 'Guarda oclusal de acrílico',
        price: 1200,
        imageUrl: productImage('provisionales-guarda-oclusal-acrilico.webp')
      }
    ]
  },
  {
    id: 'totally-natural',
    name: 'Totally Natural by tcs',
    imageUrl: productImage('totally-natural-dentadura-total.webp'),
    products: [
      {
        id: 'totally-natural-dentadura-total',
        name: 'Dentadura total c/u',
        price: 3200,
        imageUrl: productImage('totally-natural-dentadura-total.webp')
      },
      {
        id: 'totally-natural-protesis-bilateral',
        name: 'Prótesis bilateral',
        price: 2900,
        imageUrl: productImage('totally-natural-protesis-bilateral.webp')
      },
      {
        id: 'totally-natural-protesis-unilateral-1-2',
        name: 'Prótesis unilateral de 1 a 2 unidades',
        price: 1500
      },
      { id: 'totally-natural-protesis-unilateral-3', name: 'Prótesis unilateral 3 unidades', price: 1700 }
    ]
  },
  {
    id: 'iflex',
    name: 'iFlex by tcs',
    imageUrl: productImage('iflex-protesis-bilateral.webp'),
    products: [
      {
        id: 'iflex-protesis-bilateral',
        name: 'Prótesis bilateral',
        price: 2900,
        imageUrl: productImage('iflex-protesis-bilateral.webp')
      },
      {
        id: 'iflex-protesis-unilateral-1-2',
        name: 'Prótesis unilateral de 1 a 2 unidades',
        price: 1500,
        imageUrl: productImage('iflex-protesis-unilateral-1-2.webp')
      },
      { id: 'iflex-protesis-unilateral-3', name: 'Prótesis unilateral 3 unidades', price: 1700 }
    ]
  },
  {
    id: 'prostodoncia-parcial-total',
    name: 'Prostodoncia parcial y total',
    imageUrl: productImage('prostodoncia-dentadura-total-luciton.webp'),
    products: [
      {
        id: 'prostodoncia-dentadura-total-luciton',
        name: 'Dentadura total acrílico Luciton 199 c/u',
        price: 2900,
        imageUrl: productImage('prostodoncia-dentadura-total-luciton.webp')
      },
      {
        id: 'prostodoncia-dentadura-total-kulzer',
        name: 'Dentadura total en acrílico Kulzer c/u',
        price: 2700,
        imageUrl: productImage('prostodoncia-dentadura-total-kulzer.webp')
      }
    ]
  },
  {
    id: 'servicios-prostodonticos',
    name: 'Servicios prostodónticos',
    products: [
      { id: 'servicios-reparacion-dentadura-fractura', name: 'Reparación de dentadura por fractura', price: 650 },
      { id: 'servicios-gancho-volado', name: 'Gancho volado', price: 300 },
      { id: 'servicios-descanso-metalico', name: 'Descanso metálico c/u', price: 250 },
      { id: 'servicios-rebase', name: 'Rebase', price: 1100 },
      { id: 'servicios-aumentar-dientes', name: 'Aumentar dientes c/u', price: 350 }
    ]
  },
  {
    id: 'protesis-removible-metal-acrilico',
    name: 'Prótesis removible metal-acrílico',
    imageUrl: productImage('protesis-removible-unidad-metalica..webp'),
    products: [
      { id: 'protesis-removible-unidad-acrilica', name: 'Unidad acrílica', price: 180 },
      {
        id: 'protesis-removible-unidad-metalica',
        name: 'Unidad metálica',
        price: 240,
        imageUrl: productImage('protesis-removible-unidad-metalica..webp')
      }
    ]
  },
  {
    id: 'protesis-inmediata-provisional',
    name: 'Prótesis inmediata provisional',
    imageUrl: productImage('protesis-inmediata-provisional.webp'),
    products: [
      { id: 'protesis-inmediata-1-unidad', name: 'Prótesis de 1 unidad', price: 500 },
      { id: 'protesis-inmediata-1-4-unidades', name: 'Prótesis de 1 a 4 unidades', price: 900 },
      { id: 'protesis-inmediata-1-9-unidades', name: 'Prótesis de 1 a 9 unidades', price: 1300 },
      { id: 'protesis-inmediata-10-unidades', name: 'A partir de 10 unidades', price: 1450 }
    ]
  }
];
