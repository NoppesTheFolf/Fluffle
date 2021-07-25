import { Injectable } from '@angular/core';
import { Title } from '@angular/platform-browser';

@Injectable({
  providedIn: 'root'
})
export class TitleService {
  public static readonly Suffix: string = 'Fluffle';

  constructor(
    private title: Title
  ) { }

  get() {
    return this.title.getTitle();
  }

  set(value: string | null) {
    const title = value == null ? TitleService.Suffix : `${value} - ${TitleService.Suffix}`;
    this.title.setTitle(title);
  }
}
