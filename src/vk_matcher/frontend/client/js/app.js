///// <reference path="jquery-2.1.4.min.js" />

//function main() {
//    if (window.location.hash.length > 0) {
//        var arrUrl = window.location.hash.substr(1).split('/');
//        var action = arrUrl[0];
//        var arg0 = arrUrl[1];
        
//        if (action === 'proccess') {
//            function doAjax() {
//                $.ajax({
//                    type: "POST",
//                    url: "api/result",
//                    data: JSON.stringify({ taskid: arg0 }),
//                    contentType: "application/json; charset=utf-8",
//                    dataType: "json",
//                    async: true,
//                    success: function (msg) {
//                        if (!msg.d)
//                            setTimeout(doAjax, 1000);
//                        else {
//                            // Success! Notify user here
//                        }
//                    },
//                    error: function (xhr, ajaxOptions, thrownError) {
//                        alert('AJAX failure');
//                    }
//                });
//            }
            
//            setTimeout(doAjax, 1000);
//        }
//    }
//}

//window.onload = main;