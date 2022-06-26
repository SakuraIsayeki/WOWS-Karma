import { ChangeDetectionStrategy, Component } from "@angular/core";
import { FormBuilder } from "@angular/forms";
import { NgbModal } from "@ng-bootstrap/ng-bootstrap";
import { BehaviorSubject } from "rxjs";
import { map } from "rxjs/operators";
import "src/app/services/extensions";
import { ProfileService } from "../../services/api/services/profile.service";
import { AuthService } from "../../services/auth.service";
import { SeedTokenChangeComponent } from "../../shared/modals/seed-token-change/seed-token-change.component";

@Component({
    templateUrl: "./settings.component.html",
    changeDetection: ChangeDetectionStrategy.Default,
})
export class SettingsComponent {
    changesSaved$ = new BehaviorSubject(false);
    copiedTokenToClipboard$ = new BehaviorSubject(false);
    optOutOnCooldown$ = this.authService.profileFlags$.pipe(
        map((profileFlags) => {
            // Check if the user is opted out and if they are on cooldown (1 Week after opting out)
            const cooldownEnd = new Date(profileFlags?.optOutChanged!)?.addDays(7);
            const isOnCooldown = (cooldownEnd && cooldownEnd > new Date(Date.now())) ?? false;
            return { isOnCooldown, cooldownEnd };
        }),
    );

    platformBanned$ = new BehaviorSubject(false);

    form = this.formBuilder.nonNullable.group({
        id: 0,
        optedOut: false,
    });

    constructor(
        public authService: AuthService,
        private profileService: ProfileService,
        private formBuilder: FormBuilder,
        private modalService: NgbModal,
    ) {
        this.authService.profileFlags$.subscribe((profileFlags) => {
            if (profileFlags) {
                this.form.patchValue(profileFlags);

                if (profileFlags.postsBanned) {
                    this.platformBanned$.next(true);
                    this.form.disable();
                }
            }
        });
    }

    saveChanges() {
        this.profileService.apiProfilePut$Json({ body: this.form.value }).subscribe(() => {
            this.changesSaved$.next(true);
        });
    }

    async copyTokenToClipboard() {
        await window.navigator.clipboard.writeText(this.authService.authToken$.value!)
            .then(() => {
                this.copiedTokenToClipboard$.next(true);
            });
    }

    openResetSeedModal() {
        return SeedTokenChangeComponent.OpenModal(this.modalService);
    }

    toDateObject(dateNumber: number) {
        return new Date(dateNumber);
    }
}