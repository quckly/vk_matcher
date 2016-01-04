'use strict';

var app = angular.module('vkMatcherApp', ['ngRoute']);

app.config(['$routeProvider', function ($routeProvider) {
    $routeProvider.when('/', {
        templateUrl: 'page/gettoken.html',
        controller: 'GetTokenCtrl'
    }).when('/wait/:taskId', {
        templateUrl: 'page/waitresult.html',
        controller: 'WaitCtrl'
    }).when('/result/:taskId', {
        templateUrl: 'page/result.html',
        controller: 'ResultCtrl'
    }).otherwise({
        redirectTo: '/'
    });
}]);

