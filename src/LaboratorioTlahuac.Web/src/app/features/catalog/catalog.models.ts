export interface CatalogSection {
  id: string;
  key: string;
  name: string;
  description: string | null;
  imagePath: string | null;
  altText: string | null;
  sortOrder: number;
  isActive: boolean;
  createdAtUtc: string;
  updatedAtUtc: string;
}

export interface CatalogProduct {
  id: string;
  catalogSectionId: string;
  catalogSectionKey: string;
  catalogSectionName: string;
  key: string;
  name: string;
  description: string | null;
  priceAmount: number;
  currency: string;
  imagePath: string | null;
  altText: string | null;
  sortOrder: number;
  isActive: boolean;
  createdAtUtc: string;
  updatedAtUtc: string;
}

export interface CatalogSectionUpsertRequest {
  key: string;
  name: string;
  description?: string | null;
  imagePath?: string | null;
  altText?: string | null;
  sortOrder: number;
  isActive: boolean;
}

export interface CatalogProductUpsertRequest {
  catalogSectionId: string;
  key: string;
  name: string;
  description?: string | null;
  priceAmount: number;
  currency?: string | null;
  imagePath?: string | null;
  altText?: string | null;
  sortOrder: number;
  isActive: boolean;
}

export interface CatalogStatusRequest {
  isActive: boolean;
}

export interface CatalogPriceRequest {
  priceAmount: number;
  currency?: string | null;
}
