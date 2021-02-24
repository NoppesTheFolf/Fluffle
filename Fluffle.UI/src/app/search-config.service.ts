import { Injectable } from '@angular/core';

export const ImageRating = {
  Safe: "SAFE",
  Explicit: "EXPLICIT"
};

@Injectable({
  providedIn: 'root'
})
export class SearchConfigService {
  private _imageRating: string = ImageRating.Safe;

  get imageRating(): string {
    return this._imageRating;
  }

  set imageRating(value: string) {
    localStorage.setItem("rating", value);
    this._imageRating = value;
  }

  constructor() {
    if (localStorage.getItem("rating") !== null) {
      this._imageRating = localStorage.getItem("rating");
    }
  }
}
