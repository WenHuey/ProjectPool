window.onload = function () {
    const signUpButton = document.getElementById('signUp');
    const loginButton = document.getElementById('login');
    const container = document.getElementById('container');

    signUpButton.addEventListener('click', () => {
        container.classList.add("right-panel-active");
    });

    loginButton.addEventListener('click', () => {
        container.classList.remove("right-panel-active");
    });
}