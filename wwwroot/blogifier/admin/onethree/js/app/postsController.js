﻿var postsController = function (dataService) {
    function publish() {
        loading();
        var items = $('.bf-posts-list input:checked');
        for (i = 0; i < items.length; i++) {
            if (i + 1 < items.length) {
                dataService.put("blogifier/api/posts/publish/" + items[i].id, null, emptyCallback, fail);
            }
            else {
                dataService.put("blogifier/api/posts/publish/" + items[i].id, null, updateCallback, fail);
            }
        }
    }
    function unpublish() {
        loading();
        var items = $('.bf-posts-list input:checked');
        for (i = 0; i < items.length; i++) {
            if (i + 1 < items.length) {
                dataService.put("blogifier/api/posts/unpublish/" + items[i].id, null, emptyCallback, fail);
            }
            else {
                dataService.put("blogifier/api/posts/unpublish/" + items[i].id, null, updateCallback, fail);
            }
        }
    }
    function featured(id, obj) {
        var i = $(obj.firstElementChild);

        if (i.hasClass("fa-star")) {
            dataService.put("blogifier/api/posts/featured/" + id + "?act=remove", null, removeFeatured(i), fail);
        }
        else {
            dataService.put("blogifier/api/posts/featured/" + id + "?act=add", null, addFeatured(i), fail);
        }
    }
    function addFeatured(i) {
        i.removeClass('fa-star-o').addClass('fa-star');
        toastr.success("Updated");
    }
    function removeFeatured(i) {
        i.removeClass('fa-star').addClass('fa-star-o');
        toastr.success("Updated");
    }
    function removePost() {
        loading();
        var items = $('.bf-posts-list input:checked');
        for (i = 0; i < items.length; i++) {
            if (i + 1 < items.length) {
                dataService.remove('blogifier/api/posts/' + items[i].id, emptyCallback, fail);
            }
            else {
                dataService.remove('blogifier/api/posts/' + items[i].id, updateCallback, fail);
            }
        }
    }

    function emptyCallback() { }
    function updateCallback() {
        toastr.success('Updated');
        reload();
    }

    function loading() {
        $(btnAction).hide();
        $('.spin-icon').fadeIn();
    }

    function reload() {
        setTimeout(function () {
            window.location.href = webRoot + 'admin';
        }, 1000);
    }

    function filter() {
        var user = $('input[name=user-filter]:checked').val();
        if (!user) {
            user = "0";
        }
        var status = $('input[name=status-filter]:checked').val();
        var cats = $('input:checkbox:checked').map(function () {
            return this.value;
        }).get();

        var url = webRoot + "admin?user=" + user + "&status=" + status;
        if (cats.length > 0) {
            url = url + "&cats=" + cats;
        }
        window.location.href = url;
    }

    function fail(jqXHR, exception) {
        var msg = '';
        if (jqXHR.status === 0) { msg = 'Not connect.\n Verify Network.'; }
        else if (jqXHR.status == 404) { msg = 'Requested page not found. [404]'; }
        else if (jqXHR.status == 500) { msg = 'Internal Server Error [500].'; }
        else if (exception === 'parsererror') { msg = 'Requested JSON parse failed.'; }
        else if (exception === 'timeout') { msg = 'Time out error.'; }
        else if (exception === 'abort') { msg = 'Ajax request aborted.'; }
        else { msg = 'Uncaught Error.\n' + jqXHR.responseText; }
        toastr.error(msg);
    }

    return {
        publish: publish,
        unpublish: unpublish,
        removePost: removePost,
        filter: filter,
        featured: featured
    }
}(DataService);

$('.bf-posts-list .item-link-desktop').click(function () {
    $('.bf-posts-list .item-link-desktop').removeClass('active');
    $(this).addClass('active');
});

var itemCheck = $('.item-checkbox');
var firstItemCheck = itemCheck.first();
var btnAction = '#postsMultiactions';
var sidebarTools = '#sidebarTools';

// check all
$(firstItemCheck).on('change', function () {
    $(itemCheck).prop('checked', this.checked);
    toggleActionBtns();
});

// uncheck "check all" when one item is unchecked
$(itemCheck).not(firstItemCheck).on('change', function () {
    if ($(this).not(':checked')) {
        $(firstItemCheck).prop('checked', false);
    }
});

// show multi action buttons when any item checked
$('.bf-posts-list').on('change', itemCheck, function () {
    toggleActionBtns();
});
