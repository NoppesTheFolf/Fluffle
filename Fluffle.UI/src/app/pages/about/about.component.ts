import { Component } from '@angular/core';
import { ContentCenterService } from 'src/app/content-center.service';
import { SvgService } from 'src/app/svg.service';
import { TitleService } from 'src/app/title.service';

@Component({
  selector: 'app-about',
  templateUrl: './about.component.html',
  styleUrls: ['./about.component.scss']
})
export class AboutComponent {
  constructor(titleService: TitleService, contentCenterService: ContentCenterService, public svg: SvgService) {
    titleService.title = "About";
    contentCenterService.center();
  }
}
