import { Directive, ElementRef, HostListener, Input } from '@angular/core';

@Directive({
  selector: '[appDragClass]'
})
export class DragClassDirective {
  @Input('appDragClass') dragClass: any;

  constructor(public elementRef: ElementRef) {
  }

  @HostListener('dragover')
  onMouseEnter() {
    this.elementRef.nativeElement.classList.add(this.dragClass);
  }

  @HostListener('drop')
  @HostListener('dragleave')
  onMouseLeave() {
    this.elementRef.nativeElement.classList.remove(this.dragClass);
  }
}
