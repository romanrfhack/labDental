import { DOCUMENT } from '@angular/common';
import { Injectable, inject } from '@angular/core';
import { Meta, Title } from '@angular/platform-browser';
import { ActivatedRouteSnapshot, RouterStateSnapshot, TitleStrategy } from '@angular/router';

const productionOrigin = 'https://laboratoriodentaltlahuac.com';
const defaultDescription =
  'Laboratorio Dental Tláhuac: prótesis, restauraciones dentales, materiales y precios de referencia para profesionales de la salud dental.';
const publicDescriptions: Readonly<Record<string, string>> = {
  '/': 'Laboratorio Dental Tláhuac: prótesis, restauraciones dentales, materiales y precios de referencia para odontólogos, consultorios y clínicas.',
  '/catalogo': 'Consulta el catálogo de Laboratorio Dental Tláhuac por categorías, con materiales, trabajos, imágenes y precios de referencia en MXN.',
  '/servicios': 'Conoce las soluciones de Laboratorio Dental Tláhuac en restauraciones estéticas, prótesis removible, provisionales y servicios complementarios.',
  '/contacto': 'Contacta a Laboratorio Dental Tláhuac para confirmar indicaciones, disponibilidad, tiempos y precio final de tu trabajo dental.'
};

@Injectable()
export class AppTitleStrategy extends TitleStrategy {
  private readonly document = inject(DOCUMENT);
  private readonly meta = inject(Meta);
  private readonly title = inject(Title);

  override updateTitle(snapshot: RouterStateSnapshot) {
    const pageTitle = this.buildTitle(snapshot);

    if (pageTitle) {
      this.title.setTitle(pageTitle);
    }

    const leafRoute = this.getLeafRoute(snapshot.root);
    const path = this.getPath(snapshot.url);
    const description = this.getRouteDescription(leafRoute, path);
    const canonicalUrl = this.getCanonicalUrl(path);
    const isPrivateRoute = this.isPrivateRoute(path);
    const isDevelopmentHost = this.document.location.hostname.startsWith('dev.');
    const robots = isPrivateRoute || isDevelopmentHost ? 'noindex, nofollow' : 'index, follow';
    const resolvedTitle = pageTitle ?? this.title.getTitle();

    this.meta.updateTag({ name: 'description', content: description });
    this.meta.updateTag({ name: 'robots', content: robots });
    this.meta.updateTag({ property: 'og:title', content: resolvedTitle });
    this.meta.updateTag({ property: 'og:description', content: description });
    this.meta.updateTag({ property: 'og:type', content: 'website' });
    this.meta.updateTag({ property: 'og:locale', content: 'es_MX' });

    if (isPrivateRoute) {
      this.removeCanonicalLink();
      this.meta.removeTag("property='og:url'");
      return;
    }

    this.meta.updateTag({ property: 'og:url', content: canonicalUrl });
    this.updateCanonicalLink(canonicalUrl);
  }

  private getLeafRoute(route: ActivatedRouteSnapshot) {
    let currentRoute = route;

    while (currentRoute.firstChild) {
      currentRoute = currentRoute.firstChild;
    }

    return currentRoute;
  }

  private getRouteDescription(route: ActivatedRouteSnapshot, path: string) {
    const routeDescription = route.data['description'];

    if (typeof routeDescription === 'string' && routeDescription.trim()) {
      return routeDescription.trim();
    }

    return publicDescriptions[path] ?? defaultDescription;
  }

  private getPath(url: string) {
    return url.split(/[?#]/, 1)[0] || '/';
  }

  private getCanonicalUrl(path: string) {
    return `${productionOrigin}${path === '/' ? '/' : path}`;
  }

  private isPrivateRoute(path: string) {
    return path === '/login' || path === '/app' || path.startsWith('/app/');
  }

  private updateCanonicalLink(url: string) {
    let canonical = this.document.head.querySelector<HTMLLinkElement>('link[rel="canonical"]');

    if (!canonical) {
      canonical = this.document.createElement('link');
      canonical.rel = 'canonical';
      this.document.head.appendChild(canonical);
    }

    canonical.href = url;
  }

  private removeCanonicalLink() {
    this.document.head.querySelector<HTMLLinkElement>('link[rel="canonical"]')?.remove();
  }
}
