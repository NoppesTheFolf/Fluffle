import { AfterViewInit, Component, ElementRef, Input, ViewChild } from '@angular/core';
import { SearchResponseThumbnail } from '../api.service';

@Component({
  selector: 'app-gallery-thumbnail',
  templateUrl: './gallery-thumbnail.component.html',
  styleUrls: ['./gallery-thumbnail.component.scss']
})
export class GalleryThumbnailComponent implements AfterViewInit {
  @Input("thumbnail") thumbnail: SearchResponseThumbnail;
  @Input("hasBlur") hasBlur: boolean;

  @ViewChild("imageElement") image: ElementRef<HTMLImageElement>;

  hasBeenLoaded: boolean;
  error: string;

  get hasError(): boolean {
    return this.error != null;
  }

  constructor() { }

  ngAfterViewInit(): void {
    let dlImage = new Image();

    dlImage.onload = () => {
      this.image.nativeElement.src = dlImage.src;
      this.hasBeenLoaded = true;
    };

    dlImage.onerror = () => {
      this.error = "Fluffle offline";
    };

    dlImage.src = this.thumbnail.location;
  }
}
