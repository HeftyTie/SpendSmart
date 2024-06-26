$(function () {
    const $notificationElement = $('#notificationToast');
    if ($notificationElement.length > 0) {
        $notificationElement.css('display', 'block');
        const toast = new bootstrap.Toast($notificationElement[0], {
            autohide: true,
            delay: 1500
        });
        toast.show();
    }
});

function handleThemeSwitch() {
    const currentTheme = document.documentElement.getAttribute('data-bs-theme');
    const newTheme = currentTheme === 'dark' ? 'light' : 'dark';
    toggleTheme(newTheme);
}

const themeButtons = document.querySelectorAll('.theme-switch');
themeButtons.forEach(button => {
    button.addEventListener('click', handleThemeSwitch);
});

//Toggles the theme
function toggleTheme(theme) {
    document.documentElement.setAttribute('data-bs-theme', theme);
    localStorage.setItem('theme', theme); // Save the theme to local storage

    const themeIcons = document.getElementsByClassName('theme-icon');
    // Loop through all theme-icon elements and toggle classes
    for (let i = 0; i < themeIcons.length; i++) {
        if (theme === 'light') {
            themeIcons[i].classList.add('bi-brightness-high-fill');
            themeIcons[i].classList.remove('bi-moon-stars-fill');
        } else {
            themeIcons[i].classList.add('bi-moon-stars-fill');
            themeIcons[i].classList.remove('bi-brightness-high-fill');
        }
    }
}

function setInitialTheme() {
    const savedTheme = localStorage.getItem('theme');
    if (savedTheme) {
        toggleTheme(savedTheme)
    } else {
        // Check user's preferred color scheme
        const preferedScheme = window.matchMedia('(prefers-color-scheme: dark)').matches;
        toggleTheme(preferedScheme ? 'dark' : 'light');
    }
}

$(function () {
    const currentPage = location.pathname.toLowerCase();
    let matchFound = false;

    $('.nav-tabs li a').each(function () {
        const $this = $(this);
        const href = $this.attr('href').toLowerCase();

        if (currentPage.indexOf(href) !== -1) {
            $this.addClass('active');
            matchFound = true;
        } else {
            $this.removeClass('active');
        }
    });

    if (!matchFound) {
        $('.card-header').hide();
    }
});

function togglePasswordVisibility(passwordFieldId, button) {
    var passwordField = document.getElementById(passwordFieldId);
    if (passwordField.type === "password") {
        passwordField.type = "text";
        button.textContent = "Hide";
    } else {
        passwordField.type = "password";
        button.textContent = "Show";
    }
}

setInitialTheme();