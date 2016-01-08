/// <reference path="https://ajax.googleapis.com/ajax/libs/angularjs/1.4.8/angular.min.js" />

var app = angular.module('vkMatcherApp');

app.controller('GetTokenCtrl', function () {
});

app.controller('WaitCtrl', ['$http', '$routeParams', '$interval', '$window', function ($http, $routeParams, $interval, $window) {
    var wait = this;

    wait.TaskId = $routeParams.taskId;

    var checker = $interval(function () {
        //$http({
        //    method: 'POST',
        //    url: 'https://vk.quckly.ru/',
        //    data: { "taskId": wait.TaskId }
        //}).then(function successCallback(response) {
        $http.post('https://vk.quckly.ru/api/result', { 'taskId' : wait.TaskId })
        .then(function successCallback(response) {
            if (response.status == 200) {
                $interval.cancel(checker);

                $window.location.href = '#/result/' + wait.TaskId;
            }
        });
        
    }, 1000);

}]);

app.controller('ResultCtrl', ['$http', '$routeParams', function ($http, $routeParams) {
    var result = this;

    result.TaskId = $routeParams.taskId;

    $http.post('https://vk.quckly.ru/api/result', { 'taskId': result.TaskId })
    .then(function successCallback(response) {
        if (response.status == 200) {
            result.Result = response.data;
            result.ResultJson = JSON.stringify(response.data, null, 2);
            result.Avatar = response.data.PhotoMaxOrig;
        }
    });
}]);