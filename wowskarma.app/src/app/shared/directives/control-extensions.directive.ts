import { Directive, ElementRef, Host, HostBinding, HostListener, Input } from "@angular/core";
import { FormControl, FormControlDirective } from "@angular/forms";

@Directive({
    selector: '[formControlExtensions]',
    standalone: true
})
export class ControlExtensionsDirective {


  //@Input('formControlExtensions')
  //control?: FormControl;

  @Input('formControlExtensions')
  element?: HTMLElement;

  @HostBinding('class.invalid')
  get invalidClass() {
    return this.formControl?.control?.invalid && this.blurredClass;
  }

  @HostBinding('class.blurred')
  blurredClass = false;

  @HostListener('blur', ['$event'])
  blurListener(event: MouseEvent){
    console.log('blurred');
    this.blurredClass = true;
    this.element?.classList.add('blurred');
  }

  constructor(@Host() private formControl: FormControlDirective) {

  }
}
