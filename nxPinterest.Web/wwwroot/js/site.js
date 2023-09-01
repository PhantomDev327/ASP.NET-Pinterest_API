// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

// ローディングタイプ: しばらくお待ちください
function showLoadingIndicator() {
    $('body').addClass('opacity-50 pe-none');
    $('#loading-indicator').show();

}
function hideLoadingIndicator() {
    $('#loading-indicator').hide();
    $('body').removeClass('opacity-50 pe-none');
}

// ローディングタイプ: くるくる
function showLoader() {
    $('.indicator-loader').show();
}
function hideLoader() {
    setTimeout(function () {
        $('.indicator-loader').fadeOut("slow");
    }, 1500);
}
