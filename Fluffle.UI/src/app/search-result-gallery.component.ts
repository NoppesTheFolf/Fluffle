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

  constructor(public searchService: SearchResultService) { }

  ngOnInit(): void {
    this.render = this.renderGallery();
  }

  @HostListener('window:resize') onResize() {
    this.render = this.renderGallery();
  }

  renderGallery(): GalleryRow<SearchResultImage>[] {
    let gallery = new Gallery<SearchResultImage>(this.targetHeight, this.maximumHeight);
    this.images.forEach(r => {
      gallery.addImage(r.thumbnail.width, r.thumbnail.height, r);
    });
    let render = gallery.render(this.element.clientWidth - 16, 4);

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

    return render.slice(0, i + 1);
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
