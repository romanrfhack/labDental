import { DOCUMENT } from '@angular/common';
import { Injectable, inject } from '@angular/core';
import { Meta, Title } from '@angular/platform-browser';
import { ActivatedRouteSnapshot, RouterStateSnapshot, TitleStrategy } from '@angular/router';

const productionOrigin = 'https://laboratoriodentaltlahuac.com';
const defaultDescription =
  'Laboratorio Dental Tláhuac: prótesis, restauraciones dentales, materiales y precios de referencia para profesionales de la salud dental.';

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
    const description = this.getRouteDescription(leafRoute);
    const canonicalUrl = this.getCanonicalUrl(snapshot.url);
    const isPrivateRoute = this.isPrivateRoute(snapshot.url);
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

  private getRouteDescription(route: ActivatedRouteSnapshot) {
    const description = route.data['description'];

    return typeof description === 'string' && description.trim() ? description.trim() : defaultDescription;
  }

  private getCanonicalUrl(url: string) {
    const path = url.split(/[?#]/, 1)[0] || '/';
    return `${productionOrigin}${path === '/' ? '/' : path}`;
  }

  private isPrivateRoute(url: string) {
    return url === '/login' || url.startsWith('/login?') || url === '/app' || url.startsWith('/app/');
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
