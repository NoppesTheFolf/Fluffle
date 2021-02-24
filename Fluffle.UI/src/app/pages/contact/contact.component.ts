import { Component, OnInit } from '@angular/core';
import { ContentCenterService } from 'src/app/content-center.service';
import { TitleService } from 'src/app/title.service';

@Component({
  selector: 'app-contact',
  templateUrl: './contact.component.html',
  styleUrls: ['./contact.component.scss']
})
export class ContactComponent implements OnInit {
  constructor(titleService: TitleService, contentCenterService: ContentCenterService) {
    titleService.title = "Contact";
    contentCenterService.center();
  }

  ngOnInit(): void {
  }
}
