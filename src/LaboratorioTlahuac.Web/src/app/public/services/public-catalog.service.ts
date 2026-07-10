import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { map, Observable } from 'rxjs';

import { ApiClient } from '../../core/http/api-client';
import { CatalogProduct, CatalogSection } from '../data/catalog-data';

@Injectable({ providedIn: 'root' })
export class PublicCatalogService {
  private readonly http = inject(HttpClient);
  private readonly apiClient = inject(ApiClient);

  getPublicCatalog(): Observable<readonly CatalogSection[]> {
    return this.http
      .get<unknown>(this.apiClient.getUrl('/api/catalog/public'))
      .pipe(map((response) => mapPublicCatalogResponse(response)));
  }
}

export function mapPublicCatalogResponse(response: unknown): readonly CatalogSection[] {
  const payload = requireRecord(response, 'catalog');
  const rawSections = payload['sections'];

  if (!Array.isArray(rawSections) || rawSections.length === 0) {
    throw new Error('Catalog response does not contain sections.');
  }

  const sections = rawSections.map(mapPublicCatalogSection);

  if (!sections.some((section) => section.products.length > 0)) {
    throw new Error('Catalog response does not contain products.');
  }

  return sections;
}

function mapPublicCatalogSection(section: unknown): CatalogSection {
  const payload = requireRecord(section, 'catalog section');
  const rawProducts = payload['products'];

  if (!Array.isArray(rawProducts)) {
    throw new Error('Catalog section does not contain products.');
  }

  return {
    id: requireText(payload['key'], 'section key'),
    name: requireText(payload['name'], 'section name'),
    imageUrl: optionalText(payload['imagePath'], 'section imagePath'),
    altText: optionalText(payload['altText'], 'section altText'),
    products: rawProducts.map(mapPublicCatalogProduct)
  };
}

function mapPublicCatalogProduct(product: unknown): CatalogProduct {
  const payload = requireRecord(product, 'catalog product');

  return {
    id: requireText(payload['key'], 'product key'),
    name: requireText(payload['name'], 'product name'),
    price: requirePrice(payload['priceAmount'], 'product priceAmount'),
    imageUrl: optionalText(payload['imagePath'], 'product imagePath'),
    altText: optionalText(payload['altText'], 'product altText')
  };
}

function requireRecord(value: unknown, fieldName: string): Record<string, unknown> {
  if (!value || typeof value !== 'object' || Array.isArray(value)) {
    throw new Error(`Invalid ${fieldName}.`);
  }

  return value as Record<string, unknown>;
}

function requireText(value: unknown, fieldName: string): string {
  if (typeof value !== 'string' || value.trim().length === 0) {
    throw new Error(`Invalid ${fieldName}.`);
  }

  return value.trim();
}

function optionalText(value: unknown, fieldName: string): string | undefined {
  if (value === null || value === undefined) {
    return undefined;
  }

  if (typeof value !== 'string') {
    throw new Error(`Invalid ${fieldName}.`);
  }

  const normalized = value.trim();

  return normalized.length > 0 ? normalized : undefined;
}

function requirePrice(value: unknown, fieldName: string): number {
  if (typeof value !== 'number' || !Number.isFinite(value) || value < 0) {
    throw new Error(`Invalid ${fieldName}.`);
  }

  return value;
}
