// WorldlineViewer 主应用脚本

const { createApp, ref, computed, onMounted, nextTick } = Vue;

// 动作类型到中文的映射
const ACTION_TYPE_MAP = {
    'PlayCard': '出牌',
    'UsePotion': '使用药水',
    'RelicTrigger': '遗物触发',
    'PassiveEffect': '被动效果',
    'EndTurn': '结束回合',
    'StartTurn': '开始回合',
    'EnemyAction': '敌人动作',
    'DamageDealt': '造成伤害',
    'DamageReceived': '受到伤害',
    'BlockGained': '获得格挡',
    'BlockLost': '失去格挡',
    'PowerGained': '获得能力',
    'PowerLost': '失去能力',
    'CardDrawn': '抽牌',
    'CardDiscarded': '弃牌',
    'CardExhausted': '消耗牌'
};

// 动作类型到颜色的映射
const ACTION_COLOR_MAP = {
    'PlayCard': '#4CAF50',        // 绿色
    'UsePotion': '#2196F3',       // 蓝色
    'RelicTrigger': '#FF9800',    // 橙色
    'PassiveEffect': '#9C27B0',   // 紫色
    'EndTurn': '#607D8B',         // 灰色
    'StartTurn': '#795548',       // 棕色
    'EnemyAction': '#F44336',     // 红色
    'DamageDealt': '#FF5722',     // 深橙色
    'DamageReceived': '#E91E63',  // 粉色
    'BlockGained': '#00BCD4',     // 青色
    'BlockLost': '#9E9E9E',       // 浅灰色
    'PowerGained': '#8BC34A',     // 浅绿色
    'PowerLost': '#FFC107',       // 琥珀色
    'CardDrawn': '#3F51B5',       // 靛蓝色
    'CardDiscarded': '#673AB7',   // 深紫色
    'CardExhausted': '#009688'    // 青绿色
};

// 节点类
class Node {
    constructor(id, originalData, index, total) {
        this.id = id;
        this.originalData = originalData;
        this.index = index;
        this.total = total;
        
        // 计算初始位置（水平排列）
        this.x = 0;
        this.y = 0;
        
        // 节点属性
        this.radius = 30;
        this.color = ACTION_COLOR_MAP[originalData.type] || '#795548';
        this.isDragging = false;
        this.isSelected = false;
        
        // 计算摘要文本
        this.summary = this.calculateSummary();
    }
    
    calculateSummary() {
        const turn = `T${this.originalData.turnNumber}`;
        const actionType = ACTION_TYPE_MAP[this.originalData.type] || this.originalData.type;
        return `${turn}: ${actionType}`;
    }
    
    // 检查点是否在节点内
    containsPoint(x, y) {
        const dx = x - this.x;
        const dy = y - this.y;
        return dx * dx + dy * dy <= this.radius * this.radius;
    }
    
    // 绘制节点
    draw(ctx) {
        ctx.save();
        
        // 绘制节点主体
        ctx.beginPath();
        ctx.arc(this.x, this.y, this.radius, 0, Math.PI * 2);
        
        // 设置填充和描边
        if (this.isSelected) {
            ctx.fillStyle = this.color;
            ctx.strokeStyle = '#FFFFFF';
            ctx.lineWidth = 3;
        } else {
            ctx.fillStyle = this.color;
            ctx.strokeStyle = '#FFFFFF';
            ctx.lineWidth = 2;
        }
        
        ctx.fill();
        ctx.stroke();
        
        // 绘制摘要文本
        ctx.fillStyle = '#FFFFFF';
        ctx.font = 'bold 12px Arial';
        ctx.textAlign = 'center';
        ctx.textBaseline = 'middle';
        
        // 确保文本在节点内
        const maxWidth = this.radius * 1.8;
        let text = this.summary;
        
        // 如果文本太长，进行截断
        const textMetrics = ctx.measureText(text);
        if (textMetrics.width > maxWidth) {
            // 尝试缩短文本
            if (text.length > 10) {
                text = text.substring(0, 8) + '...';
            }
        }
        
        ctx.fillText(text, this.x, this.y);
        
        ctx.restore();
    }
}

// 连线类
class Connection {
    constructor(sourceNode, targetNode) {
        this.sourceNode = sourceNode;
        this.targetNode = targetNode;
    }
    
    // 绘制连线
    draw(ctx) {
        ctx.save();
        
        ctx.beginPath();
        ctx.moveTo(this.sourceNode.x, this.sourceNode.y);
        ctx.lineTo(this.targetNode.x, this.targetNode.y);
        
        ctx.strokeStyle = 'rgba(100, 100, 100, 0.5)';
        ctx.lineWidth = 2;
        ctx.lineCap = 'round';
        ctx.stroke();
        
        ctx.restore();
    }
}

// Canvas绘制引擎
class CanvasRenderer {
    constructor(canvas, nodes, connections) {
        this.canvas = canvas;
        this.ctx = canvas.getContext('2d');
        this.nodes = nodes;
        this.connections = connections;
        
        // 画布状态
        this.offsetX = 0;
        this.offsetY = 0;
        this.scale = 1.0;
        this.isPanning = false;
        this.lastMouseX = 0;
        this.lastMouseY = 0;
        
        // 初始化画布大小
        this.resizeCanvas();
        window.addEventListener('resize', () => this.resizeCanvas());
    }
    
    resizeCanvas() {
        const container = this.canvas.parentElement;
        this.canvas.width = container.clientWidth;
        this.canvas.height = container.clientHeight;
        this.render();
    }
    
    // 将屏幕坐标转换为画布坐标
    screenToCanvas(x, y) {
        return {
            x: (x - this.offsetX) / this.scale,
            y: (y - this.offsetY) / this.scale
        };
    }
    
    // 将画布坐标转换为屏幕坐标
    canvasToScreen(x, y) {
        return {
            x: x * this.scale + this.offsetX,
            y: y * this.scale + this.offsetY
        };
    }
    
    // 开始平移
    startPan(x, y) {
        this.isPanning = true;
        this.lastMouseX = x;
        this.lastMouseY = y;
    }
    
    // 平移画布
    pan(x, y) {
        if (!this.isPanning) return;
        
        const dx = x - this.lastMouseX;
        const dy = y - this.lastMouseY;
        
        this.offsetX += dx;
        this.offsetY += dy;
        
        this.lastMouseX = x;
        this.lastMouseY = y;
        
        this.render();
    }
    
    // 停止平移
    stopPan() {
        this.isPanning = false;
    }
    
    // 缩放画布
    zoom(delta, centerX, centerY) {
        const zoomFactor = 1.1;
        const oldScale = this.scale;
        
        if (delta > 0) {
            this.scale *= zoomFactor;
        } else {
            this.scale /= zoomFactor;
        }
        
        // 限制缩放范围
        this.scale = Math.max(0.1, Math.min(5, this.scale));
        
        // 调整偏移量以保持中心点
        const scaleRatio = this.scale / oldScale;
        this.offsetX = centerX - (centerX - this.offsetX) * scaleRatio;
        this.offsetY = centerY - (centerY - this.offsetY) * scaleRatio;
        
        this.render();
    }
    
    // 渲染所有内容
    render() {
        const ctx = this.ctx;
        const width = this.canvas.width;
        const height = this.canvas.height;
        
        // 清除画布
        ctx.clearRect(0, 0, width, height);
        
        // 保存当前状态
        ctx.save();
        
        // 应用变换
        ctx.translate(this.offsetX, this.offsetY);
        ctx.scale(this.scale, this.scale);
        
        // 绘制网格背景（可选）
        this.drawGrid(ctx);
        
        // 绘制所有连线
        this.connections.forEach(connection => {
            connection.draw(ctx);
        });
        
        // 绘制所有节点
        this.nodes.forEach(node => {
            node.draw(ctx);
        });
        
        // 恢复状态
        ctx.restore();
    }
    
    // 绘制网格背景
    drawGrid(ctx) {
        const gridSize = 50;
        const width = this.canvas.width / this.scale;
        const height = this.canvas.height / this.scale;
        const offsetX = -this.offsetX / this.scale;
        const offsetY = -this.offsetY / this.scale;
        
        ctx.strokeStyle = 'rgba(200, 200, 200, 0.3)';
        ctx.lineWidth = 1;
        
        // 绘制垂直线
        const startX = Math.floor(offsetX / gridSize) * gridSize;
        for (let x = startX; x < offsetX + width; x += gridSize) {
            ctx.beginPath();
            ctx.moveTo(x, offsetY);
            ctx.lineTo(x, offsetY + height);
            ctx.stroke();
        }
        
        // 绘制水平线
        const startY = Math.floor(offsetY / gridSize) * gridSize;
        for (let y = startY; y < offsetY + height; y += gridSize) {
            ctx.beginPath();
            ctx.moveTo(offsetX, y);
            ctx.lineTo(offsetX + width, y);
            ctx.stroke();
        }
    }
    
    // 查找点击的节点
    findNodeAt(x, y) {
        const canvasPos = this.screenToCanvas(x, y);
        for (let i = this.nodes.length - 1; i >= 0; i--) {
            const node = this.nodes[i];
            if (node.containsPoint(canvasPos.x, canvasPos.y)) {
                return node;
            }
        }
        return null;
    }
}

// 布局管理器
class LayoutManager {
    constructor(nodes, canvasWidth, canvasHeight) {
        this.nodes = nodes;
        this.canvasWidth = canvasWidth;
        this.canvasHeight = canvasHeight;
    }
    
    // 计算初始布局（水平排列）
    calculateInitialLayout() {
        const totalNodes = this.nodes.length;
        if (totalNodes === 0) return;
        
        // 计算水平间距
        const horizontalSpacing = Math.min(150, this.canvasWidth / (totalNodes + 1));
        const startX = horizontalSpacing;
        const centerY = this.canvasHeight / 2;
        
        // 为每个节点设置位置
        this.nodes.forEach((node, index) => {
            node.x = startX + index * horizontalSpacing;
            node.y = centerY;
            
            // 添加一些垂直偏移避免完全重叠
            if (totalNodes > 1) {
                const verticalRange = Math.min(200, this.canvasHeight * 0.6);
                const offset = (index % 3 - 1) * (verticalRange / 3);
                node.y += offset;
            }
        });
    }
    
    // 从本地存储加载节点位置
    loadPositionsFromStorage(battleId) {
        const storageKey = `worldline_viewer_positions_${battleId}`;
        const savedPositions = localStorage.getItem(storageKey);
        
        if (savedPositions) {
            try {
                const positions = JSON.parse(savedPositions);
                this.nodes.forEach((node, index) => {
                    if (positions[index]) {
                        node.x = positions[index].x;
                        node.y = positions[index].y;
                    }
                });
                return true;
            } catch (error) {
                console.error('Failed to load positions from storage:', error);
                return false;
            }
        }
        return false;
    }
    
    // 保存节点位置到本地存储
    savePositionsToStorage(battleId) {
        const positions = this.nodes.map(node => ({
            x: node.x,
            y: node.y
        }));
        
        const storageKey = `worldline_viewer_positions_${battleId}`;
        localStorage.setItem(storageKey, JSON.stringify(positions));
    }
    
    // 重置布局
    resetLayout() {
        this.calculateInitialLayout();
    }
}

// 创建Vue应用
createApp({
    setup() {
        // 响应式数据
        const battleData = ref(null);
        const selectedNode = ref(null);
        const isDragOver = ref(false);
        const uploadStatus = ref(null);
        const nodeSize = ref(40);
        const canvas = ref(null);
        
        // 计算属性
        const legendItems = computed(() => {
            return Object.entries(ACTION_TYPE_MAP).map(([type, label]) => ({
                type,
                label,
                color: ACTION_COLOR_MAP[type] || '#795548'
            }));
        });
        
        // 实例变量
        let renderer = null;
        let layoutManager = null;
        let nodes = [];
        let connections = [];
        
        // 方法
        const triggerFileInput = () => {
            document.getElementById('fileInput').click();
        };
        
        const onDragOver = (event) => {
            event.preventDefault();
            isDragOver.value = true;
        };
        
        const onDragLeave = () => {
            isDragOver.value = false;
        };
        
        const onDrop = (event) => {
            event.preventDefault();
            isDragOver.value = false;
            
            const files = event.dataTransfer.files;
            if (files.length > 0) {
                handleFile(files[0]);
            }
        };
        
        const onFileSelected = (event) => {
            const file = event.target.files[0];
            if (file) {
                handleFile(file);
            }
            // 重置文件输入，允许选择同一个文件
            event.target.value = '';
        };
        
        const handleFile = (file) => {
            // 验证文件类型
            if (!file.name.endsWith('.json')) {
                showUploadStatus('error', '请选择JSON格式的文件');
                return;
            }
            
            showUploadStatus('info', '正在读取文件...');
            
            const reader = new FileReader();
            reader.onload = (e) => {
                try {
                    const jsonData = JSON.parse(e.target.result);
                    processBattleData(jsonData);
                    showUploadStatus('success', `文件加载成功: ${file.name}`);
                } catch (error) {
                    console.error('JSON解析错误:', error);
                    showUploadStatus('error', 'JSON文件格式错误');
                }
            };
            
            reader.onerror = () => {
                showUploadStatus('error', '文件读取失败');
            };
            
            reader.readAsText(file);
        };
        
        const processBattleData = (data) => {
            // 验证数据格式，支持两种命名约定
            const hasCamelCase = data.battleId && data.actions && Array.isArray(data.actions);
            const hasPascalCase = data.BattleId && data.Actions && Array.isArray(data.Actions);
            
            if (!hasCamelCase && !hasPascalCase) {
                showUploadStatus('error', '无效的战斗记录格式');
                return;
            }
            
            // 标准化数据格式（统一转换为camelCase）
            const normalizedData = normalizeDataFormat(data);
            battleData.value = normalizedData;
            
            // 创建节点和连线
            createVisualization(normalizedData);
            
            // 初始化渲染器
            initRenderer();
            
            // 清除选中状态
            selectedNode.value = null;
        };
        
        const createVisualization = (data) => {
            nodes = [];
            connections = [];
            
            // 创建节点
            data.actions.forEach((action, index) => {
                const node = new Node(
                    `node_${index}`,
                    action,
                    index,
                    data.actions.length
                );
                nodes.push(node);
                
                // 创建连线（连接到前一个节点）
                if (index > 0) {
                    const connection = new Connection(nodes[index - 1], node);
                    connections.push(connection);
                }
            });
        };
        
        const initRenderer = () => {
            if (!canvas.value) return;
            
            // 创建布局管理器
            layoutManager = new LayoutManager(
                nodes,
                canvas.value.width,
                canvas.value.height
            );
            
            // 尝试从本地存储加载位置
            const positionsLoaded = layoutManager.loadPositionsFromStorage(battleData.value.battleId);
            
            // 如果未加载到位置，计算初始布局
            if (!positionsLoaded) {
                layoutManager.calculateInitialLayout();
            }
            
            // 创建渲染器
            renderer = new CanvasRenderer(canvas.value, nodes, connections);
            renderer.render();
        };
        
        const showUploadStatus = (type, message) => {
            const icons = {
                success: 'fas fa-check-circle',
                error: 'fas fa-exclamation-circle',
                info: 'fas fa-info-circle'
            };
            
            uploadStatus.value = {
                type,
                message,
                icon: icons[type]
            };
            
            // 5秒后清除状态消息
            setTimeout(() => {
                uploadStatus.value = null;
            }, 5000);
        };
        
        const formatDateTime = (dateTimeStr) => {
            try {
                const date = new Date(dateTimeStr);
                return date.toLocaleString('zh-CN');
            } catch (error) {
                return dateTimeStr;
            }
        };
        
        const getActionTypeChinese = (actionType) => {
            return ACTION_TYPE_MAP[actionType] || actionType;
        };
        
        const onCanvasMouseDown = (event) => {
            if (!renderer) return;
            
            const rect = canvas.value.getBoundingClientRect();
            const x = event.clientX - rect.left;
            const y = event.clientY - rect.top;
            
            // 检查是否点击了节点
            const clickedNode = renderer.findNodeAt(x, y);
            if (clickedNode) {
                // 选中节点
                if (selectedNode.value) {
                    selectedNode.value.isSelected = false;
                }
                clickedNode.isSelected = true;
                selectedNode.value = clickedNode;
                
                // 开始拖拽节点
                clickedNode.isDragging = true;
                renderer.render();
            } else {
                // 开始平移画布
                renderer.startPan(x, y);
            }
        };
        
        const onCanvasMouseMove = (event) => {
            if (!renderer) return;
            
            const rect = canvas.value.getBoundingClientRect();
            const x = event.clientX - rect.left;
            const y = event.clientY - rect.top;
            
            // 检查是否有节点正在被拖拽
            const draggingNode = nodes.find(node => node.isDragging);
            if (draggingNode) {
                const canvasPos = renderer.screenToCanvas(x, y);
                draggingNode.x = canvasPos.x;
                draggingNode.y = canvasPos.y;
                renderer.render();
            } else {
                // 平移画布
                renderer.pan(x, y);
            }
        };
        
        const onCanvasMouseUp = () => {
            if (!renderer) return;
            
            // 停止拖拽节点
            nodes.forEach(node => {
                if (node.isDragging) {
                    node.isDragging = false;
                    
                    // 保存位置到本地存储
                    if (battleData.value && layoutManager) {
                        layoutManager.savePositionsToStorage(battleData.value.battleId);
                    }
                }
            });
            
            // 停止平移画布
            renderer.stopPan();
        };
        
        const onCanvasWheel = (event) => {
            if (!renderer) return;
            
            event.preventDefault();
            
            const rect = canvas.value.getBoundingClientRect();
            const x = event.clientX - rect.left;
            const y = event.clientY - rect.top;
            
            renderer.zoom(event.deltaY, x, y);
        };
        
        const resetLayout = () => {
            if (!layoutManager || !renderer) return;
            
            layoutManager.resetLayout();
            renderer.render();
            
            // 清除本地存储的位置
            if (battleData.value) {
                const storageKey = `worldline_viewer_positions_${battleData.value.battleId}`;
                localStorage.removeItem(storageKey);
            }
        };
        
        const clearData = () => {
            battleData.value = null;
            selectedNode.value = null;
            nodes = [];
            connections = [];
            renderer = null;
            layoutManager = null;
            
            // 清除画布
            if (canvas.value) {
                const ctx = canvas.value.getContext('2d');
                ctx.clearRect(0, 0, canvas.value.width, canvas.value.height);
            }
        };
        
        const copyNodeDetails = () => {
            if (!selectedNode.value) return;
            
            const details = JSON.stringify(selectedNode.value.originalData, null, 2);
            navigator.clipboard.writeText(details)
                .then(() => {
                    showUploadStatus('success', '节点详情已复制到剪贴板');
                })
                .catch(err => {
                    console.error('复制失败:', err);
                    showUploadStatus('error', '复制失败，请手动复制');
                });
        };
        
        // 数据标准化函数
        const normalizeDataFormat = (data) => {
            // 如果已经是camelCase格式，直接返回
            if (data.battleId && data.actions) {
                return data;
            }
            
            // 转换PascalCase到camelCase
            const normalized = {
                battleId: data.BattleId || data.battleId,
                startTime: data.StartTime || data.startTime,
                endTime: data.EndTime || data.endTime,
                totalTurns: data.TotalTurns || data.totalTurns,
                actions: []
            };
            
            // 转换动作数组
            const actions = data.Actions || data.actions || [];
            normalized.actions = actions.map(action => ({
                turnNumber: action.TurnNumber || action.turnNumber,
                timestamp: action.Timestamp || action.timestamp,
                type: action.Type || action.type,
                initiator: action.Initiator || action.initiator,
                target: action.Target || action.target,
                cardId: action.CardId || action.cardId,
                potionId: action.PotionId || action.potionId,
                relicId: action.RelicId || action.relicId,
                isPassive: action.IsPassive || action.isPassive,
                damageAmount: action.DamageAmount || action.damageAmount,
                blockAmount: action.BlockAmount || action.blockAmount,
                metadata: action.Metadata || action.metadata || {}
            }));
            
            return normalized;
        };
        
        // 生命周期钩子
        onMounted(() => {
            // 初始化画布大小
            if (canvas.value) {
                const container = canvas.value.parentElement;
                canvas.value.width = container.clientWidth;
                canvas.value.height = container.clientHeight;
            }
            
            // 添加键盘快捷键
            document.addEventListener('keydown', (event) => {
                if (event.key === 'Escape' && selectedNode.value) {
                    selectedNode.value.isSelected = false;
                    selectedNode.value = null;
                    if (renderer) renderer.render();
                }
                
                if (event.key === 'r' && event.ctrlKey && battleData.value) {
                    event.preventDefault();
                    resetLayout();
                }
                
                if (event.key === 'Delete' && battleData.value) {
                    event.preventDefault();
                    clearData();
                }
            });
        });
        
        return {
            // 数据
            battleData,
            selectedNode,
            isDragOver,
            uploadStatus,
            nodeSize,
            canvas,
            
            // 计算属性
            legendItems,
            
            // 方法
            triggerFileInput,
            onDragOver,
            onDragLeave,
            onDrop,
            onFileSelected,
            formatDateTime,
            getActionTypeChinese,
            onCanvasMouseDown,
            onCanvasMouseMove,
            onCanvasMouseUp,
            onCanvasWheel,
            resetLayout,
            clearData,
            copyNodeDetails
        };
    }
}).mount('#app');