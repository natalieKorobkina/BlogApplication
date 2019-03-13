$(function () {
    $(".actionDelete").on('click', function (e) {
        e.preventDefault();
        var result = confirm("Are you sure you want to delete this post?");
        if (result) {
            $(this).closest('form').submit();
        }
    });
});
$(function () {
    $(".actionEdit").on('click', function (e) {
        e.preventDefault();
        $(this).closest('form').submit();
    });
});