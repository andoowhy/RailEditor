using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

[CustomEditor( typeof( Rail ) )]
public class RailEditor : Editor
{

    public void OnEnable()
    {
        Rail rail = target as Rail;
        UpdateCollisionMesh( rail );        
    }

    public override void OnInspectorGUI()
    {
        Rail rail = target as Rail;

		//Always have a minumum of two nodes
		if( rail.nodes.Count < 2 )
		{
			rail.nodes.Clear();
			rail.nodes.Add( Vector3.one );
			rail.nodes.Add( 2 * Vector3.one );
			UpdateCollisionMesh( rail );
		}

        GUILayout.BeginHorizontal();
        {
            //Number of Nodes
            GUILayout.Label( "Nodes" );
            GUILayout.TextArea( rail.nodes.Count.ToString() );

            //Add and Remove Nodes
            if( GUILayout.Button( "+" ) )
            {
                Undo.RecordObject( rail, "Add Node" );
                Vector3 node = new Vector3();
                node = rail.nodes[ rail.nodes.Count - 1 ] + Vector3.one;
                rail.nodes.Add( node );
                UpdateCollisionMesh( rail );
                EditorUtility.SetDirty( rail );
            }
            if( GUILayout.Button( "-" ) && rail.nodes.Count > 2 )
            {
                Undo.RecordObject( rail, "Remove Node" );
                rail.nodes.RemoveAt( rail.nodes.Count - 1 );
                UpdateCollisionMesh( rail );
                EditorUtility.SetDirty( rail );
            }
        }        
        GUILayout.EndHorizontal();

        if( GUI.changed )
        {
            EditorUtility.SetDirty( target );
        }
    }

    public void OnSceneGUI()
    {
        Rail rail = target as Rail;

		if( Tools.current == Tool.Move )
		{
			//Draw Move Handles
	        for(int i = 0; i < rail.nodes.Count; i++)
	        {
	            Vector3 handlePos = rail.transform.TransformPoint( rail.nodes[i] );
	            
                EditorGUI.BeginChangeCheck();
                handlePos = Handles.PositionHandle( handlePos, Quaternion.identity );
                if( EditorGUI.EndChangeCheck() )
                {
                    Undo.RecordObject( rail, "Move Rail Node" );
                    EditorUtility.SetDirty( rail );
                    rail.nodes[i] = rail.transform.InverseTransformPoint( handlePos );
                    UpdateCollisionMesh( rail );
                }
            }
		}

        //Draw Lines between nodes in Scene View
        for( int i = 0; i < rail.nodes.Count - 1; i++ )
        {
            Debug.DrawLine( rail.transform.TransformPoint( rail.nodes[i] ),
                            rail.transform.TransformPoint( rail.nodes[i + 1] ),
                            Color.magenta );
        }

        if( GUI.changed )
        {
           EditorUtility.SetDirty( target );
        }
    }

    private void UpdateCollisionMesh( Rail rail )
    {
        MeshCollider mc = rail.GetComponent<Collider>() as MeshCollider;

        List<Vector3> verts = new List<Vector3>();
        List<int> tris = new List<int>();

        for( int i = 0; i < rail.nodes.Count; i++ )
        {
            Vector3 lookDir;
            if( i <= 0 )
            {
                lookDir = rail.nodes[1] - rail.nodes[0];
            }
            else if( i > 0 && i < rail.nodes.Count - 1 )
            {
                lookDir = ( rail.nodes[i + 1] - rail.nodes[i] ).normalized + ( rail.nodes[i] - rail.nodes[i - 1] ).normalized / 2f;
            }
            else
            {
                lookDir =  rail.nodes[rail.nodes.Count - 1] - rail.nodes[rail.nodes.Count - 2];
            }

            Quaternion lootRot = Quaternion.LookRotation( lookDir );

            //Create vert triangle
            verts.Add( new Vector3( -0.1f, 0.05f, 0f ) );
            verts.Add( new Vector3( 0.1f, 0.05f, 0f ) );
            verts.Add( new Vector3( 0f, -0.05f, 0f ) );

            //Move vert triangle to node
            for( int j = verts.Count - 3; j < verts.Count; j++ )
            {
                verts[j] = verts[j] + rail.nodes[i];
            }

            //Point first triangle to next node
            for( int j = verts.Count - 3; j < verts.Count; j++ )
            {
                Vector3 vertDir = verts[j] - rail.nodes[i];
                vertDir = lootRot * vertDir;
                verts[j] = vertDir + rail.nodes[i];
            }          
        }

        //Build the triangles
        
        //Add first node end triangle
        tris.Add( 0 );
        tris.Add( 1 );
        tris.Add( 2 );
        for( int i = 0; i < rail.nodes.Count - 1; i++ )
        {
            //Top
            tris.Add( 3 * i + 3 );
            tris.Add( 3 * i + 1 );
            tris.Add( 3 * i + 0 );

            tris.Add( 3 * i + 4 );
            tris.Add( 3 * i + 1 );
            tris.Add( 3 * i + 3 );

            //Right
            tris.Add( 3 * i + 4 );
            tris.Add( 3 * i + 5 );
            tris.Add( 3 * i + 1 );

            tris.Add( 3 * i + 1 );
            tris.Add( 3 * i + 5 );
            tris.Add( 3 * i + 2 );

            //Left
            tris.Add( 3 * i + 3 );
            tris.Add( 3 * i + 0 );
            tris.Add( 3 * i + 5 );

            tris.Add( 3 * i + 5 );
            tris.Add( 3 * i + 0 );
            tris.Add( 3 * i + 2 );
        }

        //Add last node end triangle
        tris.Add( verts.Count - 1 );
        tris.Add( verts.Count - 2 );
        tris.Add( verts.Count - 3 );

		Mesh mesh = new Mesh();
		mesh.vertices = verts.ToArray();
		mesh.triangles = tris.ToArray();

		mc.sharedMesh = mesh;
    }
}
