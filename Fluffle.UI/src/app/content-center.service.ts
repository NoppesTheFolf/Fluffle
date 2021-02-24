import { Injectable } from '@angular/core';
import { Subject } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class ContentCenterService {
  onCenter = new Subject<boolean>();

  center() {
    this.onCenter.next(true);
  }
}
