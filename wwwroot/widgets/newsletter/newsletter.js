var newsletter = function () {

    function subscribe() {
        var email = document.getElementById("txtSubscriberEmail");
        alert(email.value);
    }

    return {
        subscribe: subscribe
    }
}();
