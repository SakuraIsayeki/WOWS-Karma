import { Directive, ElementRef, HostBinding, HostListener, Input, inject } from "@angular/core";
import { FormControl, FormControlDirective } from "@angular/forms";

@Directive({
    selector: '[formControlExtensions]',
    standalone: true
})
export class ControlExtensionsDirective {
  private formControl = inject(FormControlDirective, { host: true });



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

  /** Inserted by Angular inject() migration for backwards compatibility */
  constructor(...args: unknown[]);

  constructor() {

  }
}
