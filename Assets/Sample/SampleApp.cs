using System.Collections.Generic;
using UnityEngine;
using Goisagi;

/// <summary>
/// 
/// </summary>
public class SampleApp : MonoBehaviour
{
    [SerializeField] FallThroughHandler fallThroughHandler = null;

  // Start is called before the first frame update
    void Start()
    {
        // コールバック登録.
        FallThroughHandler fth = fallThroughHandler;
        fth.onPress += OnPress;
        fth.onBeginPress += OnBeginPress;
        fth.onEndPress += OnEndPress;

        fth.onDrag += OnDrag; 
        fth.onBeginDrag += OnBeginDrag;
        fth.onEndDrag += OnEndDrag;

        fth.onPinch += OnPinch;
        fth.onBeginPinch += OnBeginPinch;
        fth.onEndPinch += OnEndPinch;

        fth.onFlick += OnFlick;
        fth.onDoubleClick += OnDoubleClick;
    }

    void OnDestroy()
    {
        // コールバック登録解除.
        FallThroughHandler fth = fallThroughHandler;
        fth.onPress -= OnPress;
        fth.onBeginPress -= OnBeginPress;
        fth.onEndPress -= OnEndPress;

        fth.onDrag -= OnDrag; 
        fth.onBeginDrag -= OnBeginDrag;
        fth.onEndDrag -= OnEndDrag;

        fth.onPinch -= OnPinch;
        fth.onBeginPinch -= OnBeginPinch;
        fth.onEndPinch -= OnEndPinch;

        fth.onFlick -= OnFlick;
        fth.onDoubleClick -= OnDoubleClick;
    }

    void OnPress( bool isDown ){ AddGUILog("OnPress"); }
    void OnBeginPress( Vector2 pressPos ){ AddGUILog("OnBeginPress"); }
    void OnEndPress( Vector2 releasePos ){ AddGUILog("OnEndPress"); }

    void OnDrag( Vector2 delta, Vector2 dragPos ){ AddGUILog("OnDrag"); }
    void OnBeginDrag( Vector2 dargPos ){ AddGUILog("OnBeginDrag"); }
    void OnEndDrag( Vector2 releasePos ){ AddGUILog("OnEndDrag"); }

    void OnPinch( Vector2 beginPos, Vector2 endPos, float pinchRange ){ AddGUILog("OnPinch"); }
    void OnBeginPinch( Vector2 beginPos, Vector2 endPos ){ AddGUILog("OnBeginPinch"); }
    void OnEndPinch(){ AddGUILog("OnEndPinch"); }

    void OnFlick( Vector2 releasePos, Vector2 delta, FlickDirType flickType ){ AddGUILog("OnFlick"); }
    void OnDoubleClick( Vector2 releasePos ){ AddGUILog("OnDoubleClick"); }


#region DEBUG_GUI_LOG
    const int DISPLAY_LOG_MAX = 30;
    List<string> logList = new List<string>();
    GUIStyle guiStyle = new GUIStyle();

    void OnGUI()
    {
        guiStyle.fontSize = ( int )(Screen.height / (DISPLAY_LOG_MAX * 2));
        foreach( var item in logList ){
            GUILayout.Label( item, guiStyle );
        }
    }

    void AddGUILog( string text )
    {
        if( logList.Count > DISPLAY_LOG_MAX ){
            logList.RemoveAt( 0 );
        }

        logList.Add( text );
    }
#endregion // DEBUG_GUI_LOG


}   // End of class SampleApp.
