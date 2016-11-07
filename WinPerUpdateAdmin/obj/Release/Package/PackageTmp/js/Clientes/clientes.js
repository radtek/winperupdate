﻿/// <reference path="serviceClientes.js" />
/// <reference path="serviceClientes.js" />
(function () {
    'use strict';

    angular
        .module('app')
        .controller('clientes', clientes);

    clientes.$inject = ['$scope', '$window', '$routeParams', 'serviceClientes'];

    function clientes($scope, $window, $routeParams, serviceClientes) {
        $scope.title = 'clientes';
        $scope.regiones = [];
        $scope.comunas = [];

        activate();

        function activate() {
            $scope.idCliente = 0;
            $scope.titulo = "Crear Cliente";
            $scope.labelcreate = "Crear un Cliente";
            $scope.increate = true;
            $scope.rutok = true;
            $scope.formData = {};
            $scope.mensaje = '';
            $scope.totales = [0, 0];
            $scope.usuarios = [];
            $scope.estadosMantencion = [{ valor: 7, nomest: "Activo" }, { valor: 9, nomest: "No Activo" }];
            $scope.mesesInicioMantencion = [{ valor: "01", nommes: "Enero" },
            { valor: "02", nommes: "Febrero" },
            { valor: "03", nommes: "Marzo" },
            { valor: "04", nommes: "Abril" },
            { valor: "05", nommes: "Mayo" },
            { valor: "06", nommes: "Junio" },
            { valor: "07", nommes: "Julio" },
            { valor: "08", nommes: "Agosto" },
            { valor: "09", nommes: "Septiembre" },
            { valor: "10", nommes: "Octubre" },
            { valor: "11", nommes: "Noviembre" },
            { valor: "12", nommes: "Diciembre" }, ];

            
            serviceClientes.getRegiones().success(function (regiones) {
                $scope.regiones = regiones;
            }).error(function (error) {
                console.error(data);
            });

            

            if (!jQuery.isEmptyObject($routeParams)) {
                $scope.idCliente = $routeParams.idCliente;
                $scope.titulo = "Modificar Cliente";
                $scope.labelcreate = "Modificar";

                serviceClientes.getCliente($scope.idCliente).success(function (data) {
                    $scope.formData.rut = data.RutFmt;
                    $scope.formData.nombre = data.Nombre;
                    $scope.formData.direccion = data.Direccion;
                    $scope.formData.region = data.Comuna.Region.idRgn;
                    $scope.formData.licencia = data.NroLicencia;
                    $scope.formData.folio = data.NumFolio;
                    $scope.formData.estmtc = data.EstMtc;
                    $scope.formData.mesini = data.Mesini;
                    $scope.formData.nrotrbc = data.NroTrbc;
                    $scope.formData.nrotrbh = data.NroTrbh;
                    $scope.formData.nrousr = data.NroUsr;

                    

                    serviceClientes.getComunas(data.Comuna.Region.idRgn).success(function (data2) {
                        $scope.comunas = data2;

                        $scope.formData.comuna = data.Comuna.idCmn;
                    });

                    serviceClientes.getUsuarios($scope.idCliente).success(function (data) {
                        $scope.usuarios = data;

                        angular.forEach($scope.usuarios, function (item) {
                            $scope.totales[item.CodPrf - 11]++;
                        });

                    }).error(function (data) {
                        console.error(data);
                    });

                }).error(function (data) {
                    console.error(data);
                });


            } else {
                serviceClientes.getFolio().success(function (data) {
                    $scope.formData.folio = data;
                }).error(function (err) {
                    console.log(err);
                });
            }

            $scope.Comunas = function (formData) {
                serviceClientes.getComunas(formData.region).success(function (data) {
                    $scope.comunas = data;
                });
            }

            $scope.ShowConfirm = function () {
                $("#delete-modal").modal('show');
            }

            $scope.ValidarRut = function (formData) {
                var _value = formData.rut;
                if (_value.length < 2) return;

                _value = _value.replace(/[^0-9kK]+/g, '').toUpperCase();

                var result = _value.slice(-4, -1) + '-' + _value.substr(_value.length - 1);
                for (var i = 4; i < _value.length; i += 3) result = _value.slice(-3 - i, -i) + '' + result;

                formData.rut = result;

                var t = parseInt(_value.slice(0, -1), 10), m = 0, s = 1;
                while (t > 0) {
                    s = (s + t % 10 * (9 - m++ % 6)) % 11;
                    t = Math.floor(t / 10);
                }
                var v = (s > 0) ? (s - 1) + '' : 'K';
                $scope.rutok = (v === _value.slice(-1));
            }

            $scope.SaveCliente = function (formData) {
                console.log(JSON.stringify(formData));
                var arrRut = formData.rut.split('-');
                $scope.increate = false;
                $scope.labelcreate = "Enviando";

                if ($scope.idCliente == 0) {
                    serviceClientes.addCliente(arrRut[0], arrRut[1], formData.nombre, formData.direccion, formData.comuna, formData.licencia, formData.folio, formData.estmtc, formData.mesini, formData.nrotrbc, formData.nrotrbh, formData.nrousr).success(function (data) {
                        $scope.increate = true;
                        $scope.labelcreate = "Modificar";

                        console.log(JSON.stringify(data));
                        $scope.idCliente = data.Id;
                    }).error(function (data) {
                        console.error(data);
                    });
                }
                else {
                    serviceClientes.updCliente($scope.idCliente, arrRut[0], arrRut[1], formData.nombre, formData.direccion, formData.comuna, formData.licencia, formData.folio, formData.estmtc, formData.mesini, formData.nrotrbc, formData.nrotrbh, formData.nrousr).success(function (data) {
                        $scope.increate = true;
                        $scope.labelcreate = "Modificar";

                        console.log(JSON.stringify(data));
                    }).error(function (data) {
                        console.error(data);
                    });
                }
            }

            $scope.Eliminar = function () {
                serviceClientes.delCliente($scope.idCliente).success(function () {
                    $('.close').click();

                    $window.setTimeout(function () {
                        $window.location.href = "/Clientes#/";
                    }, 2000);

                }).error(function (data) {
                    console.debug(data);
                });
            }

            $scope.GenKey = function (formOK, formData) {
                if (formOK) {
                    console.log(formData.folio + "" + formData.estmtc + "" + formData.mesini + "" + formData.nrotrbc + "" + formData.nrotrbh + "" + formData.nrousr)
                    serviceClientes.getKey(formData.folio, formData.estmtc, formData.mesini, formData.nrotrbc, formData.nrotrbh, formData.nrousr).success(function (data) {
                        formData.licencia = data;
                    }).error(function (err) {
                        console.log(err);
                    });
                } else {
                    $("#genkey-modal").modal('show');
                }
            }

        }

    }
})();
