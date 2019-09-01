# fall-through-handler
UI以外のタッチ制御を行うクラス

### Unity Version
Unity 2019.1.0f2

### 使い方
1. Hierarchyに適当なGameObjectを作成  
2. GameObjectにFallThroughHandlerコンポーネントを追加  
3. Inspectorの各パラメータを設定(詳細は後述)  
4. FallThroughHandlerへ受け取りたい入力処理に応じたコールバックを設定  

### パラメータ

|名前|説明|  
|:---|:---|  
|TargetCanvas|対象Canvas,シーン再生時にこのCanvasの子要素になる|  
|FlickDetectDistance|フリック入力扱いにする移動量|  
|DoubleClickInterval|ダブルクリックの受付間隔(秒)|  

### 補足
- 対象CanvasにGraphicRaycasterが無い場合はダミーを追加
- UI関連の入力制御を妨げないために子要素追加時に transform.SetAsFirstSibling() を使い描画順を最奥に設定
- コールバックの設定方法は SampleApp.cs 内に記述