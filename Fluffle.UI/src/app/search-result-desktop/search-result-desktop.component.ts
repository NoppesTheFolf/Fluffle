import { Component, Input } from '@angular/core';
import { SearchResultService } from '../search-result.service';

@Component({
  selector: 'app-search-result-desktop',
  templateUrl: './search-result-desktop.component.html',
  styleUrls: ['./search-result-desktop.component.scss']
})
export class SearchResultDesktopComponent {
  @Input() public element: HTMLElement;

  constructor(public searchService: SearchResultService) {
  }

}
