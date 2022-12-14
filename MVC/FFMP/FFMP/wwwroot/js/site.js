// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
function togglePassword() {
    var input = document.getElementById('password');
    var icon = document.getElementById('icon')

    if (input.type === "password") {
        input.type = "text"
        icon.classList.add('selected');
    } else {
        input.type = "password";
        icon.classList.remove('selected')
    }
}