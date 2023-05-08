function setThemeDisplay(theme) {
    if (theme === void 0) { theme = localStorage['theme']; }
    if (theme === 'light') {
        document.querySelector("#light-theme").removeAttribute('disabled');
        document.querySelector("#dark-theme").setAttribute('disabled', '');
    }
    else {
        document.querySelector("#dark-theme").removeAttribute('disabled');
        document.querySelector("#light-theme").setAttribute('disabled', '');
    }
}
function getThemeSelectorIcon(theme) {
    if (theme === 'light') {
        return 'bi bi-moon';
    }
    else {
        return 'bi bi-sun';
    }
}
function setTheme(theme) {
    if (theme === void 0) { theme = localStorage['theme']; }
    //Invert theme
    theme = theme === 'light' ? 'dark' : 'light';
    localStorage['theme'] = theme;
    document.querySelector("#icon-theme-selector").setAttribute('class', getThemeSelectorIcon(theme));
    setThemeDisplay();
}
window.addEventListener('DOMContentLoaded', function () { return setThemeDisplay; });
