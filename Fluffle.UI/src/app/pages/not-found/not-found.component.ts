import { Component } from '@angular/core';
import { TitleService } from 'src/app/title.service';

@Component({
  selector: 'app-not-found',
  templateUrl: './not-found.component.html',
  styleUrls: ['./not-found.component.scss']
})
export class NotFoundComponent {
  constructor(title: TitleService) {
    title.set("Not found");
  }
}
