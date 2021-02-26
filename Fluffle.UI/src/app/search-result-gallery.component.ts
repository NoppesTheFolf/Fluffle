import { Directive, HostListener, Input, OnInit } from "@angular/core";
import { environment } from "src/environments/environment";
import { SearchResultImage, SearchResultImageMatch } from "./api.service";
import { Gallery, GalleryRow } from "./gallery";
import { SearchResultService } from "./search-result.service";

@Directive()
export class SearchResultGalleryComponent implements OnInit {
  protected environment = environment;
  @Input() element: HTMLElement;
  @Input() images: SearchResultImage[];
  @Input() targetHeight: number;
  @Input() maximumHeight: number;
  render: GalleryRow<SearchResultImage>[];
  renderWidth: number;

  constructor(public searchService: SearchResultService) { }

  ngOnInit(): void {
    this.renderGallery();
  }

  @HostListener('window:resize') onResize() {
    this.renderGallery();
  }

  renderGallery(): void {
    let newRenderWidth = this.element.clientWidth;
    if (newRenderWidth == this.renderWidth) {
      return;
    }

    let gallery = new Gallery<SearchResultImage>(this.targetHeight, this.maximumHeight);
    this.images.forEach(r => {
      let aspectRatio = r.thumbnail.width / r.thumbnail.height;

      let width = r.thumbnail.width;
      let height = r.thumbnail.height;

      // Images which are either very tall or wide, can screw with the gallery's
      // ability to fit them nicely in the grid. So, in order to fix that, we
      // force those images to be displayed as squares instead, just like on the mobile UI.
      if (aspectRatio < 0.6 || aspectRatio > 2) {
        width = 250;
        height = 250;
      }

      gallery.addImage(width, height, r);
    });
    let render = gallery.render(newRenderWidth - 16, 4);

    let minNumberOfImages = 12;
    let minNumberOfRows = 3;
    let currentNumberOfImages = 0;
    let i = 0;
    for (; i < render.length; i++) {
      var currentRow = render[i];
      currentNumberOfImages += currentRow.images.length;

      if (currentNumberOfImages >= minNumberOfImages && minNumberOfRows <= i + 1) {
        break;
      }
    }

    this.renderWidth = newRenderWidth;
    this.render = render.slice(0, i + 1);
  }
}

export class ImageHelper {
  static getLabel(image: SearchResultImage): string {
    if (image.match === SearchResultImageMatch.Excellent) {
      return "bg-success";
    }

    if (image.match === SearchResultImageMatch.Doubtful) {
      return "bg-warning";
    }

    return "bg-danger";
  }

  static getLogoLocation(image: SearchResultImage): string {
    return '/assets/img/' + image.platform + '.svg';
  }
}
