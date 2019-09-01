using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

using System.Collections.Generic;
using System;

namespace Goisagi
{

/// <summary>
/// UI以外のタッチ制御クラス.
/// </summary>
public class FallThroughHandler : 
	MonoBehaviour,
	IPointerDownHandler,
	IPointerUpHandler,
	IDragHandler,
	IBeginDragHandler,
	IEndDragHandler
{
    public const float UPDATING_FLICK_POSITION_RANGE = 10f;

    // 対象キャンバス.
    [SerializeField] Canvas m_targetCanvas = null;
    // フリック入力扱いにする移動量.
    [SerializeField] float m_flickDetectDistance = 20f;
    // ダブルクリック/タップの受付間隔(秒).
    [SerializeField] float m_doubleClickInterval = 1.0f;

    // ドラッグ＆ピンチ用.
	private Dictionary<int, PointerEventData>	m_dragEventDict = new Dictionary<int, PointerEventData>();
	private float m_beforePinchRange = 0.0f;
    Vector2 m_beginDragPos = Vector2.zero;
    Vector2 m_prevDragPos = Vector2.zero;

    // ダブルタップ用.
    private float m_pointerUpTime = 0.0f;

	private bool m_isPressing = false;
	private bool m_isDragging = false;
	private bool m_isPinching = false;

	// 押しているか.
	public bool	IsPressing { get { return m_isPressing; } }
	// ドラッグ中か？.
	public bool IsDragging { get { return m_isDragging; } }
	// ピンチ中か？.
	public bool IsPinching { get { return m_isPinching; } }

	// 各種コールバック.
	public DelegatePress onPress = delegate { };
	public DelegateBeginPress onBeginPress = delegate { };
	public DelegateEndPress onEndPress = delegate { };

	public DelegateDrag onDrag = delegate { }; 
	public DelegateBeginDrag onBeginDrag = delegate { };
	public DelegateEndDrag onEndDrag = delegate { };

	public DelegatePinch onPinch = delegate { };
	public DelegateBeginPinch onBeginPinch = delegate { };
	public DelegateEndPinch	onEndPinch = delegate { };

    public DelegateFlick onFlick = delegate { };
    public DelegateDoubleClick onDoubleClick = delegate{ };


	/// <summary>
	/// 
	/// </summary>
	void Awake()
	{
		try
		{
			// 対象Canvasが設定されているか.
			Canvas trargetCanvas = m_targetCanvas;
			if( trargetCanvas == null ){
				throw new Exception( "targetCanvas is null, please attach targetCanvas." );
			}

			// GraphicRaycasterが設定されているか.無ければ仮のものをつける.
			if( trargetCanvas.GetComponent<GraphicRaycaster>() == null ){
                Debug.LogWarning( "GraphicRaycaster was not found in TargetCanvas, so it added a new GraphicRaycaster instead." );
				trargetCanvas.gameObject.AddComponent<GraphicRaycaster>();
			}

			Image img = this.gameObject.GetComponent<Image>();
			if( img == null ){
				img = this.gameObject.AddComponent<Image>();
			}
			img.color = new Color( 0, 0, 0, 0 );
			img.raycastTarget = true;

			CanvasRenderer cr = this.gameObject.GetComponent<CanvasRenderer>();
			if( cr == null ){
				cr = this.gameObject.AddComponent<CanvasRenderer>();
			}
			cr.cullTransparentMesh = true; // 描画しないようにする.

			RectTransform rct = this.gameObject.GetComponent<RectTransform>();
			if( rct == null ){
				rct = this.gameObject.AddComponent<RectTransform>();
			}
			rct.sizeDelta = new Vector2( Screen.width, Screen.height );

			// 対象Canvasの子要素として登録.
			Transform trs = this.transform;
			trs.SetParent( trargetCanvas.transform );
			trs.localPosition = Vector3.zero;
			trs.SetAsFirstSibling(); // UIの入力をブロックしないために最奥に設置する.
		}
		catch( Exception ex )
		{
			Debug.LogError( "Failed to initialize FallThroughHandler!! " + ex.Message );
			this.gameObject.SetActive( false );
		}
    }

	/// <summary>
	/// ドラッグ入力処理が有効か.
	/// </summary>
	private bool CheckValidDrag()
	{
		#if UNITY_EDITOR
			return m_isPressing;
		#else
			return (Input.touches.Length <= 1);
		#endif // UNITY_EDITOR
	}

	/// <summary>
	/// 押した時のコールバック.
	/// </summary>
	public void OnPointerDown( PointerEventData eventData )
	{
        m_beginDragPos = eventData.position;
        m_prevDragPos = eventData.position;

		m_isPressing = true;
		onBeginPress( eventData.position );
		onPress( true );
	}

	/// <summary>
	/// 話した時のコールバック.
	/// </summary>
	public void OnPointerUp( PointerEventData eventData )
	{
		m_isPressing = false;
		onEndPress( eventData.position );
		onPress( false );

		// ピンチフラグは最後に下ろす.
		if( m_isPinching && m_dragEventDict.Count < 2 ){
			m_isPinching = false;
			onEndPinch();
		}
        else
        {
            Vector2 delta = (eventData.position - m_prevDragPos);
            float sqrtDist = (m_flickDetectDistance * m_flickDetectDistance);
            if( delta.sqrMagnitude >= sqrtDist )
            {
                // フリック方向 取得.
                FlickDirType flickType;
                float dx = Mathf.Abs( delta.x );
                float dy = Mathf.Abs( delta.y );
                if( dx > dy ){
                    flickType = (delta.x > 0.0f ? FlickDirType.Right : FlickDirType.Left);
                }else{
                    flickType = (delta.y > 0.0f ? FlickDirType.Up : FlickDirType.Down);
                }

                // フリック通知.
                onFlick( eventData.position, delta, flickType );
            }
            else
            {
                // ダブルクリック検出用.
                float prevTime = m_pointerUpTime;
                if( prevTime > 0.0f )
                {
                    m_pointerUpTime = Time.realtimeSinceStartup;

                    // 指定した間隔内ならダブルクリックとみなして通知.
                    float deltaTime = (m_pointerUpTime - prevTime);
                    if( deltaTime <= m_doubleClickInterval ){
                        onDoubleClick( eventData.position );
                    }
                    m_pointerUpTime = 0.0f;
                }
                else{
                    m_pointerUpTime = Time.realtimeSinceStartup;
                }
            }
        }
	}


	/// <summary>
	/// ドラッグ中のコールバック.
	/// </summary>
	public void OnDrag( PointerEventData eventData )
	{
		// ドラッグ開始判定.
		if( !m_isDragging ){
			OnBeginDrag( eventData );
			return;
		}

		m_dragEventDict[ eventData.pointerId ] = eventData;

		// ドラッグイベントが２つ以上ある時はピンチ.
		if( m_dragEventDict.Count >= 2 )
		{
			// ドラッグ中だった場合は終了.
			if( m_isDragging ){
				m_isDragging = false;
				onEndDrag( eventData.position );
			}

			OnPinch();
		}
		else if( CheckValidDrag())
		{
			// ピンチ中ならピンチの終了処理を行う.
			if( m_isPinching )
			{
				m_isDragging = false;
				m_isPinching = false;
				onEndPinch();

				OnBeginDrag( eventData );
			}

            // フリック制御用に座標更新.
            Vector2 delta = eventData.delta;
            float sqrtDist = (UPDATING_FLICK_POSITION_RANGE * UPDATING_FLICK_POSITION_RANGE);
            if( delta.sqrMagnitude <= sqrtDist ){
                m_prevDragPos = eventData.position;
            }

			onDrag( delta, eventData.position );
		}
	}

	/// <summary>
	/// ドラッグ開始時のコールバック.
	/// </summary>
	public void OnBeginDrag( PointerEventData eventData )
	{
		m_isDragging = true;
		m_dragEventDict[ eventData.pointerId ] = eventData;
        
		onBeginDrag( eventData.position );
	}

	/// <summary>
	/// ドラッグ終了時のコールバック.
	/// </summary>
	public void OnEndDrag( PointerEventData eventData )
	{
		m_isDragging = false;
		onEndDrag( eventData.position );

		if( m_dragEventDict.ContainsKey( eventData.pointerId )) {
			m_dragEventDict.Remove( eventData.pointerId );
		}
	}

	/// <summary>
	/// ピンチ処理
	/// </summary>
	private void OnPinch()
	{
		Vector2 touchFirst = Vector2.zero;
		Vector2 touchSecond = Vector2.zero;

		int count = 0;
		foreach( var dragData in m_dragEventDict )
		{
			if( count == 0 ){
				touchFirst = dragData.Value.position;
				count++;
			}else{
				touchSecond = dragData.Value.position;
				break;
			}
		}

		// ピンチ幅.
		float nowPinchRange = Vector2.Distance( touchFirst, touchSecond );

		// ピンチ処理.
		if( IsPinching ){
			onPinch( touchFirst, touchSecond, (nowPinchRange - m_beforePinchRange) );
			m_beforePinchRange = nowPinchRange;
		}
		else
		{
			// 初回.
			m_isPinching = true;
			m_beforePinchRange = nowPinchRange;
			onBeginPinch( touchFirst, touchSecond );
		}
	}

}   // End of class FallThroughHandler.


// フリック方向.
public enum FlickDirType
{
    Up,
    Down,
    Right,
    Left,
}

// 各種コールバックのdelegate定義.
// Press
public delegate void DelegatePress( bool isDown );
public delegate void DelegateBeginPress( Vector2 pressPos );
public delegate void DelegateEndPress( Vector2 releasePos );
// Drag
public delegate void DelegateDrag( Vector2 delta, Vector2 dragPos ); 
public delegate void DelegateBeginDrag( Vector2 dragPos );
public delegate void DelegateEndDrag( Vector2 dragPos );
// Pinch
public delegate void DelegatePinch( Vector2 beginPos, Vector2 endPos, float pinchRange );
public delegate void DelegateBeginPinch( Vector2 beginPos, Vector2 endPos );
public delegate void DelegateEndPinch();
// Others.
public delegate void DelegateFlick( Vector2 releasePos, Vector2 delta, FlickDirType flickType );
public delegate void DelegateDoubleClick( Vector2 releasePos );


} // namespace Goisagi.
