import { HTTP_INTERCEPTORS, HttpErrorResponse, HttpHandler, HttpInterceptor, HttpRequest } from "@angular/common/http";
import { ErrorHandler, Injectable, Injector, ModuleWithProviders, NgModule } from "@angular/core";
import { catchError, throwError } from "rxjs";

export class GlobalErrorHandler implements ErrorHandler {

  handleError(error: any): void {
    console.error(error);
  }
}

@Injectable()
export class ErrorInterceptor implements HttpInterceptor {

  constructor(private injector: Injector) {
  }

  intercept(req: HttpRequest<any>, next: HttpHandler): ReturnType<HttpInterceptor['intercept']> {
    return next.handle(req).pipe(catchError(err => {
      if (err instanceof HttpErrorResponse) {

      }
      return throwError(err);
    }));
  };
}


@NgModule({
  providers: [
    { provide: HTTP_INTERCEPTORS, useClass: ErrorInterceptor, multi: true },
    { provide: ErrorHandler, useClass: GlobalErrorHandler }
  ]
})
export class ErrorModule {
  static withConfig(): ModuleWithProviders<ErrorModule> {
    return {
      ngModule: ErrorModule,
      providers: [

      ]
    };
  }
}
