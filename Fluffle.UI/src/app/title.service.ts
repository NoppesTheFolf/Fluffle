import { Injectable } from '@angular/core';
import { Title } from '@angular/platform-browser';
import { NavigationEnd, NavigationStart, Router } from '@angular/router';

@Injectable({
  providedIn: 'root'
})
export class TitleService {
  public static readonly Suffix: string = 'Fluffle';

  constructor(router: Router, private titleService: Title) {
    this.title = null;

    let startTitle: string;
    router.events.subscribe(event => {
      if (event instanceof NavigationStart) {
        startTitle = this.title;
      }

      if (event instanceof NavigationEnd) {
        // The loaded component didn't change the title
        if (startTitle === this.title) {
          this.title = null;
        }
      }
    });
  }

  get title(): string {
    return this.titleService.getTitle();
  }

  set title(value: string) {
    let title = value == null ? TitleService.Suffix : `${value} - ${TitleService.Suffix}`;
    this.titleService.setTitle(title);
  }
}
