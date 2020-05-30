
var $taskVue = new Vue({
    el: "#task-container",
    data: {
        select: {
            currentRow: [],
            rows: {}
        },
        selectCom: {
            model: 'post',
            data: [{ value: 'post', label: 'post' }, { value: 'get', label: 'get' }]
        },
        log: {
            index: 0,
            model: false,
            title: '日志记录',
            group: '',
            closable: true,
            footerHide: true,
            spin: false,
            page: 0,
            data: []
        },
        isAdd: true,
        closable: false,
        footerHide: true,
        model: false,
        modelMessage: '任务管理',
        activedIndex: 0,
        taskValidate: {
            taskName: '', groupName: '', interval: '', apiUrl: '', authKey: '', authValue:
                '', describe: '', requestType: ''
        },
        ruleValidate: {
            taskName: [{ required: true, message: '作业名称必填', trigger: 'blur' }],
            groupName: [{ required: true, message: '分组名称必填', trigger: 'blur' }],
            interval: [{ required: true, message: '间隔(Cron)必填', trigger: 'blur' }],
            requestType: [{ required: true, message: '请选择请求方式', trigger: 'change' }],
            apiUrl: [{ required: true, message: 'ApiUrl必填', trigger: 'blur' }]
        },
        taskForm: [
            { name: 'taskName', text: '作业名称', value: '', placeholder: '作业名称与触发器名称默认相同', readOnly: false },
            { name: 'groupName', text: '分组', value: '', placeholder: '分组名称与分组名称默认相同', readOnly: false },
            { name: 'interval', text: '间隔(Cron)', value: '', placeholder: '如10分钟执行一次：0/0 0/10 * * * ? ' },
            { name: 'apiUrl', text: 'ApiUrl', value: '', placeholder: "远程调用接口URL" },
            { name: 'authKey', text: 'header(key)', value: '', placeholder: '请求header验证的Key' },
            { name: 'authValue', text: 'header(value)', value: '', placeholder: '请求header验证的Key' },
            {
                name: 'requestType', text: '请求方式', value: '', onChange: (data,value) => {
                }, placeholder: 'post/get', type: 'select'
            },
            { name: 'describe', text: '描述', value: '', type: 'textarea' }
        ],
        columns: [
            {
                hidden:true,
                title: '测试',
                key: 'id',
                width: 120
            },
            {
                type: 'selection',
                width: 60,
                align: 'center'
            },
            {
                title: '作业名称',
                key: 'taskName',
                width: 150
            }, {
                title: '分组',
                key: 'groupName',
                width: 120
            },
            {
                title: '最后执行时间',
                key: 'lastRunTime',
                width: 170
            }, {
                title: '间隔(Cron)',
                key: 'interval',
                width: 140
            },
            {
                title: '状态',
                key: 'status',
                width: 80,
                render: (h, params) => {
                    var style = { color: 'white', background: 'red', padding: '3px 10px', borderRadius: '4px' };
                    var text = '';
                    switch (params.row.status) {
                        case 0:
                            style.background = '#0acb0a';
                            text = '正常';
                            break;
                        case 1:
                            style.background = '#ed4014';
                            text = '暂停';
                            break;
                        case 2:
                            style.background = '#fc2f2f';
                            text = '完成';
                            break;
                        case 3:
                            style.background = '#607D8B';
                            text = '异常';
                            break;
                        case 4:
                            style.background = '#607D8B';
                            text = '阻塞';
                            break;
                        case 5:
                            style.background = '#607D8B';
                            text = '停止';
                            break;
                        default:
                            style.background = '#f90';
                            text = '不存在';
                            break;
                    }
                    return h('div', [
                        h('Button', {
                            props: {
                                //type: 'error',
                                size: 'small'
                            }, style: style,
                            on: {
                                click: function () {
                                }
                            }
                        }, text)
                    ]);
                }
            },
            {
                title: '描述',
                key: 'describe',
                minWidth:200
            },
            {
                title: 'ApiUrl',
                key: 'apiUrl',
                width: 180
            },
            {
                title: '请求方式',
                key: 'requestType',
                width: 95
            },
            {
                title: '操作',
                key: 'operat',
                width: 100,
                render: (h, params) => {
                    var style = { 'font-size': '12px' };

                    return h('div', [
                        h('i-button', {
                            props: {
                                //type: 'error',
                                size: 'small'

                            }, style: style,
                            on: {
                                click: function () {
                                    $taskVue.getJobRunLog(params);
                                }
                            }
                        }, '执行记录')
                    ]);
                }
            }
        ],
        rows: []
    }, methods: {
        onChange(item, value) {
            if (item.onChange && typeof item.onChange == "function") {
                item.onChange(value, item);
            }
        },
        getColumns: function () {
            var columns = [];
            this.columns.forEach(function (item) {
                if (!item.hidden) {
                    columns.push(item);
                }
            })
            return columns;
        },
        selectRow: function (selection, row) {
            this.select.currentRow = row;
            this.select.rows = selection;
        },
        first: function () {
            $taskVue.log.index = 0;
            $taskVue.log.page = 0;
            $taskVue.log.data = [];
            this.getJobRunLog(null, true);
        },
        next: function () {
            this.getJobRunLog(null, true);
        },
        getJobRunLog: function (params, next) {
            if (!next) {
                //if (!(params.row.taskName === $taskVue.log.title
                //    && params.row.groupName === $taskVue.log.groupName)) {
                $taskVue.log.page = 0;
                $taskVue.log.index = 0;
                $taskVue.log.title = params.row.taskName;
                $taskVue.log.groupName = params.row.groupName;
                $taskVue.log.data = [];
                //  }
            }
            $taskVue.log.model = true;
            //  $taskVue.log.spin = true;
            $taskVue.log.page++;
            $taskVue.ajax("/TaskBackGround/GetRunLog", {
                taskName: $taskVue.log.title, groupName: $taskVue.log.groupName, page: $taskVue.log.page
            }, function (data) {
                if (data.length === 0) {
                    if ($taskVue.log.page >= 1) {
                        $taskVue.log.page--;
                    }
                    if (next) {
                        $taskVue.$Message.success('未查到数据!');
                    }
                    return;
                }
                //  $taskVue.log.spin = false;
                if (next) {
                    $taskVue.log.data = data;
                    // $taskVue.log.data.push(...data);
                } else {
                    $taskVue.log.data = data;
                }
                $taskVue.log.index += $taskVue.log.index ? data.length: 1;
            });
        },
        getTaskValidate: function () {
        },
        add: function () {
            for (var key in this.taskValidate) {
                this.taskValidate[key] = '';
            }
            this.setFormClass(false);
            this.model = true;
        },
        tiggerAction: function (action) {
            if (!this.select.rows.length)
                return $taskVue.$Message.success('请选择作业!');
            this.ajax('/TaskBackGround/' + action,
                this.select.rows[0], function (data) {
                    if (data.status) {
                        $taskVue.refresh(true);
                    }
                    return $taskVue.$Message.success(data.msg);
                });
        },
        update: function () {
            //   this.tiggerAction('update');
            if (!this.select.rows.length)
                return $taskVue.$Message.success('请选择作业!');
            this.model = true;
            //this.taskValidate = this.select.rows.slice(0, 1)[0];
            for (var key in this.select.rows[0]) {
                this.taskValidate[key] = this.select.rows[0][key];
            }
            this.setFormClass(true);
            //this.taskForm[0]
        },
        refresh: function (_init) {
            this.select.currentRow = [];
            this.select.rows = {};
            this.ajax("/TaskBackGround/GetJobs", {}, function (data) {
                data.forEach(function (row) {
                    row.cellClassName = { operat: 'view-log' };
                });
                $taskVue.rows = data;
                if (!_init) {
                    return $taskVue.$Message.success('刷新成功!');
                }
            });
        },
        handleSelectAll(status) {
            this.$refs.selection.selectAll(status);
        },
        handleSubmit(name) {
            this.$refs[name].validate((valid) => {
                if (!valid) {
                    return this.$Message.error('数据填写不完整!');
                }
                this.ajax("/TaskBackGround/" + (this.isAdd ? 'add' : 'update'), this.taskValidate, function (data) {
                    $taskVue.$Message.success(data.msg || '保存成功');
                    if (data.status) {
                        $taskVue.model = false;
                        $taskVue.refresh(true);
                    }
                });
            });

        },
        setFormClass: function (readOnly) {
            this.isAdd = !readOnly;
            this.modelMessage = !readOnly ? '新建任务' : '修改任务';
            this.taskForm.forEach(x => {
                if (x.name === "taskName" || x.name === "groupName") {
                    x.readOnly = readOnly;
                }
            });
        },
        ajax: function (url, params, fun) {
            axios({
                method: 'post',
                url: url,
                params: params,
                headers: { 'X-Requested-With':'XMLHttpRequest'}
            }).then(function (response) {
                fun && fun(response.data);
                }).catch(function (error) {
                    if (error.response.status === 401) {
                        return window.location.href = '/home/index';
                    }
                $taskVue.$Message.success('出错啦!');
                console.log(error);
            });
        }
    }, created: function () {
        this.refresh(true);
    }, mounted: function () {
        //$headerVue.activedIndex = 0;
    }
});