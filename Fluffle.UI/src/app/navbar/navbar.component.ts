import { Component } from '@angular/core';
import { SvgService } from '../svg.service';

@Component({
  selector: 'app-navbar',
  templateUrl: './navbar.component.html',
  styleUrls: ['./navbar.component.scss']
})
export class NavbarComponent {

  constructor(public svg: SvgService) { }

}
