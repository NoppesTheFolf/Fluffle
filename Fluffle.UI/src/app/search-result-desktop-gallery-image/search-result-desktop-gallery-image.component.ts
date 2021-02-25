import { Component, Input } from '@angular/core';
import { SearchResultImage, SearchResultImageMatch } from '../api.service';
import { GalleryImage } from '../gallery';
import { ImageHelper } from '../search-result-gallery.component';
import { SearchResultService } from '../search-result.service';

@Component({
  selector: 'app-search-result-desktop-gallery-image',
  templateUrl: './search-result-desktop-gallery-image.component.html',
  styleUrls: ['./search-result-desktop-gallery-image.component.scss']
})
export class SearchResultDesktopGalleryImageComponent {
  @Input() image: GalleryImage<SearchResultImage>;
  SearchResultImageMatch = SearchResultImageMatch;
  ImageHelper = ImageHelper;

  hasHover: boolean = false;

  get hasBlur(): boolean {
    if (!this.searchResultService.hideImprobable) {
      return false;
    }

    if (this.image.data.match === SearchResultImageMatch.Excellent) {
      return false;
    }

    return !this.hasHover;
  }

  constructor(private searchResultService: SearchResultService) {
  }
}