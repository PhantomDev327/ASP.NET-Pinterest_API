// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

function showLoadingIndicator() {
    $('body').addClass('opacity-50 pe-none');
    $('#loading-indicator').show();

}

function hideLoadingIndicator() {
    $('#loading-indicator').hide();
    $('body').removeClass('opacity-50 pe-none');
}
