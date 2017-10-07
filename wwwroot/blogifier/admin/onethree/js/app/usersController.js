var itemCheck = $('.item-checkbox');
var firstItemCheck = itemCheck.first();

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