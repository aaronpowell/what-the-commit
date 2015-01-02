/// <reference path="app.js" />
(function (app) {
    'use strict';

    app.controller('searchController', searchController);

    searchController.$inject = ['$scope', '$http', '$rootScope'];

    function searchController($scope, $http, $rootScope) {
        /* jshint validthis:true */

        $scope.results = [];

        $scope.search = function () {
            var term = $scope.model.term;

            $http({
                url: '/search',
                data: {
                    term: term
                },
                method: 'POST'
            })
            .then(function (data) {
                $scope.results = data.data;
            }, function (err) {
                console.error(err);
                alert('Error!');
            });
        };
    }
})(window.app);
