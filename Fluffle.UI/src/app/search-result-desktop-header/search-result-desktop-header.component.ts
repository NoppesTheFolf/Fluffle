import { Component } from '@angular/core';
import { SearchResultService } from '../search-result.service';

@Component({
  selector: 'app-search-result-desktop-header',
  templateUrl: './search-result-desktop-header.component.html',
  styleUrls: ['./search-result-desktop-header.component.scss']
})
export class SearchResultDesktopHeaderComponent {  
  constructor(public searchService: SearchResultService) { }

  ngOnInit(): void {
  }
}
