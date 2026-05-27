import { isPlatformBrowser } from '@angular/common';
import {
  AfterViewInit,
  Directive,
  ElementRef,
  Inject,
  NgZone,
  OnDestroy,
  PLATFORM_ID
} from '@angular/core';

const reducedMotionQuery = '(prefers-reduced-motion: reduce)';

type ParallaxItem = {
  depth: number;
  element: HTMLElement;
};

@Directive({
  selector: '[appPublicScrollAnimations]',
  standalone: true
})
export class PublicScrollAnimationsDirective implements AfterViewInit, OnDestroy {
  private animationFrame = 0;
  private mediaQuery?: MediaQueryList;
  private observer?: IntersectionObserver;
  private parallaxItems: ParallaxItem[] = [];

  private readonly onMotionPreferenceChange = () => {
    if (this.mediaQuery?.matches) {
      this.disableMotion();
    }
  };

  private readonly onScroll = () => this.requestParallaxUpdate();
  private readonly onResize = () => this.requestParallaxUpdate();

  constructor(
    private readonly elementRef: ElementRef<HTMLElement>,
    private readonly ngZone: NgZone,
    @Inject(PLATFORM_ID) private readonly platformId: object
  ) {}

  ngAfterViewInit() {
    if (!isPlatformBrowser(this.platformId)) {
      return;
    }

    const root = this.elementRef.nativeElement;
    const revealTargets = Array.from(root.querySelectorAll<HTMLElement>('[data-animate]'));
    const parallaxTargets = Array.from(root.querySelectorAll<HTMLElement>('[data-parallax]'));

    if (!revealTargets.length && !parallaxTargets.length) {
      return;
    }

    this.mediaQuery = window.matchMedia(reducedMotionQuery);
    this.mediaQuery.addEventListener('change', this.onMotionPreferenceChange);

    if (this.mediaQuery.matches || !('IntersectionObserver' in window)) {
      this.showStaticState();
      return;
    }

    root.classList.add('public-animation-ready');

    this.ngZone.runOutsideAngular(() => {
      this.setupReveal(revealTargets);
      this.setupParallax(parallaxTargets);
    });
  }

  ngOnDestroy() {
    this.observer?.disconnect();
    this.observer = undefined;
    this.removeParallaxListeners();
    this.mediaQuery?.removeEventListener('change', this.onMotionPreferenceChange);
    this.mediaQuery = undefined;
  }

  private setupReveal(targets: HTMLElement[]) {
    if (!targets.length) {
      return;
    }

    const groupCounters = new Map<Element, number>();

    this.observer = new IntersectionObserver(
      (entries) => {
        for (const entry of entries) {
          if (!entry.isIntersecting) {
            continue;
          }

          const target = entry.target as HTMLElement;
          target.classList.add('is-visible');
          this.observer?.unobserve(target);
        }
      },
      {
        rootMargin: '0px 0px -12% 0px',
        threshold: 0.14
      }
    );

    for (const target of targets) {
      if (target.dataset['animate'] === 'stagger-card') {
        const group = target.parentElement ?? this.elementRef.nativeElement;
        const groupIndex = groupCounters.get(group) ?? 0;

        groupCounters.set(group, groupIndex + 1);
        target.style.setProperty('--reveal-delay', `${Math.min(groupIndex, 5) * 70}ms`);
      }

      this.observer.observe(target);
    }
  }

  private setupParallax(targets: HTMLElement[]) {
    this.parallaxItems = targets
      .map((element) => ({
        depth: this.getParallaxDepth(element),
        element
      }))
      .filter((item) => item.depth > 0);

    if (!this.parallaxItems.length) {
      return;
    }

    window.addEventListener('scroll', this.onScroll, { passive: true });
    window.addEventListener('resize', this.onResize);
    this.updateParallax();
  }

  private getParallaxDepth(element: HTMLElement) {
    const value = Number(element.dataset['parallax']);

    if (!Number.isFinite(value)) {
      return 0.16;
    }

    return Math.max(0.04, Math.min(value, 0.32));
  }

  private requestParallaxUpdate() {
    if (this.animationFrame) {
      return;
    }

    this.animationFrame = window.requestAnimationFrame(() => this.updateParallax());
  }

  private updateParallax() {
    this.animationFrame = 0;

    const viewportHeight = window.innerHeight || 1;

    for (const item of this.parallaxItems) {
      const rect = item.element.getBoundingClientRect();

      if (rect.bottom < 0 || rect.top > viewportHeight) {
        continue;
      }

      const elementCenter = rect.top + rect.height / 2;
      const progress = (elementCenter - viewportHeight / 2) / viewportHeight;
      const shift = Math.max(-14, Math.min(14, progress * -54 * item.depth));

      item.element.style.setProperty('--parallax-y', `${shift.toFixed(2)}px`);
    }
  }

  private disableMotion() {
    this.observer?.disconnect();
    this.observer = undefined;
    this.removeParallaxListeners();
    this.elementRef.nativeElement.classList.remove('public-animation-ready');
    this.showStaticState();
  }

  private showStaticState() {
    const targets = this.elementRef.nativeElement.querySelectorAll<HTMLElement>('[data-animate], [data-parallax]');

    for (const target of targets) {
      target.classList.add('is-visible');
      target.style.removeProperty('--parallax-y');
      target.style.removeProperty('--reveal-delay');
    }
  }

  private removeParallaxListeners() {
    if (!isPlatformBrowser(this.platformId)) {
      return;
    }

    window.removeEventListener('scroll', this.onScroll);
    window.removeEventListener('resize', this.onResize);

    if (this.animationFrame) {
      window.cancelAnimationFrame(this.animationFrame);
      this.animationFrame = 0;
    }

    this.parallaxItems = [];
  }
}
