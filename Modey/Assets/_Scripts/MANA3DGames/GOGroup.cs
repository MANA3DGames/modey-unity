using UnityEngine;
using System.Collections.Generic;
using TMPro;

namespace MANA3DGames
{
    public class GOGroup
    {
        protected GameObject root;
        public bool IsActive { get { return root.activeSelf; } }

        Dictionary<string, GameObject> items;
        public Dictionary<string, GameObject> Items { get { return items; } }


        public GOGroup( GameObject root )
        {
            this.root = root;
            this.items = new Dictionary<string, GameObject>();

            if ( root.transform.childCount > 0 )
            {
                for ( int i = 0; i < root.transform.childCount; i++ )
                {
                    items.Add( root.transform.GetChild(i).name, 
                                    root.transform.GetChild(i).gameObject );
                }
            }
        }

        public void ShowRoot( bool show )
        {
            root.SetActive( show );
        }

        public void ShowAllItems( bool show )
        {
            foreach ( var item in items )
                item.Value.SetActive( show );
        }

        public void ShowItem( string name, bool show )
        {
            GameObject component = null;
            if ( items.TryGetValue( name, out component ) )
                component.SetActive( show );
            else
                Debug.Log( name + " couldn't be found in " + root.name );
        }

        public GameObject GetRoot()
        {
            return root;
        }

        public bool GetActive( string name )
        {
            GameObject component = null;
            if ( items.TryGetValue( name, out component ) )
                return component.activeSelf;
            
            return false;
        }

        public GameObject Get( string name )
        {
            GameObject component = null;
            items.TryGetValue( name, out component );
            return component;
        }

        public SpriteRenderer GetSpriteRenderer( string name )
        {
            GameObject component = null;
            items.TryGetValue( name, out component );
            return component.GetComponent<SpriteRenderer>();
        }

        public bool Contains( string name )
        {
            GameObject component = null;
            items.TryGetValue( name, out component );
            return component != null;
        }

        public void SetScale( string name, Vector3 scale )
        {
            GameObject component = null;
            if ( items.TryGetValue( name, out component ) )
                component.transform.localScale = scale;
            else
                Debug.Log( name + " couldn't be found in " + root.name );
        }

        public void SetAllScale( Vector3 scale )
        {
            foreach ( var item in items )
                item.Value.transform.localScale = scale;
        }

        public Vector3 GetLocalScale( string name )
        {
            GameObject component = null;
            if ( items.TryGetValue( name, out component ) )
                return component.transform.localScale;
            else
                return Vector3.zero;
        }

        public void SetPosX( string name, float x )
        {
            GameObject component = null;
            if ( items.TryGetValue( name, out component ) )
                component.transform.localPosition = new Vector3( x,
                                                                 component.transform.localPosition.y,
                                                                 component.transform.localPosition.z );
            else
                Debug.Log( name + " couldn't be found in " + root.name );
        }
        public void SetPosY( string name, float y )
        {
            GameObject component = null;
            if ( items.TryGetValue( name, out component ) )
                component.transform.localPosition = new Vector3( component.transform.localPosition.x,
                                                                 y,
                                                                 component.transform.localPosition.z );
            else
                Debug.Log( name + " couldn't be found in " + root.name );
        }

        public float GetPosX( string name )
        {
            GameObject component = null;
            if ( items.TryGetValue( name, out component ) )
                return component.transform.localPosition.x;
            else
                return 0;
        }
        public float GetPosY( string name )
        {
            GameObject component = null;
            if ( items.TryGetValue( name, out component ) )
                return component.transform.localPosition.y;
            else
                return 0;
        }

        public void SetSprite( string name, Sprite sprite, bool inner = false )
        {
            GameObject component = null;
            if ( items.TryGetValue( name, out component ) )
            {
                if ( inner )
                    component.transform.Find( name ).GetComponent<SpriteRenderer>().sprite = sprite;
                else
                    component.GetComponent<SpriteRenderer>().sprite = sprite;
            }
        }

        public void SetSpriteAlpha( string name, float alpha, bool inner = false )
        {
            GameObject component = null;
            if ( items.TryGetValue( name, out component ) )
            {
                SpriteRenderer renderer = null;
                renderer = inner ? component.transform.Find( name ).GetComponent<SpriteRenderer>() : component.GetComponent<SpriteRenderer>();
                renderer.color = new Color( renderer.color.r, renderer.color.g, renderer.color.b, alpha );
            }
        }

        public void SetSpriteColor( string name, Color color, bool inner = false )
        {
            GameObject component = null;
            if ( items.TryGetValue( name, out component ) )
            {
                SpriteRenderer renderer = null;
                renderer = inner ? component.transform.Find( name ).GetComponent<SpriteRenderer>() : component.GetComponent<SpriteRenderer>();
                renderer.color = color;
            }
        }


        public void AdjustSpriteLayerOrderBy( string name, int val, bool inner = false )
        {
            GameObject component = null;
            if ( items.TryGetValue( name, out component ) )
            {
                if ( inner )
                    component.transform.Find( name ).GetComponent<SpriteRenderer>().sortingOrder += val;
                else
                    component.GetComponent<SpriteRenderer>().sortingOrder += val;
            }
        }

        public void SetSpriteLayerOrder( string name, int val, bool inner = false )
        {
            GameObject component = null;
            if ( items.TryGetValue( name, out component ) )
            {
                if ( inner )
                    component.transform.Find( name ).GetComponent<SpriteRenderer>().sortingOrder = val;
                else
                    component.GetComponent<SpriteRenderer>().sortingOrder = val;
            }
        }

        public void SetAllSpritesLayerBackBy( int val )
        {
            foreach ( var item in items )
            {
                SpriteRenderer renderer = item.Value.GetComponent<SpriteRenderer>();
                if ( renderer )
                    renderer.sortingOrder -= val;
            }
        }


        public void SetText( string name, string text, bool inner = false )
        {
            GameObject component = null;
            if ( items.TryGetValue( name, out component ) )
            {
                if ( inner )
                    component.transform.Find( name ).GetComponent<TextMeshPro>().text = text;
                else
                    component.GetComponent<TextMeshPro>().text = text;
            }
        }

        public void SetInnerText( string name, string text )
        {
            GameObject component = null;
            if ( items.TryGetValue( name, out component ) )
                component.transform.Find( "text" ).GetComponent<TextMeshPro>().text = text;
        }

        public string GetText( string name, bool inner = false )
        {
            GameObject component = null;
            if ( items.TryGetValue( name, out component ) )
            {
                if ( inner )
                    return component.transform.Find( name ).GetComponent<TextMeshPro>().text;
                else
                    return component.GetComponent<TextMeshPro>().text;
            }

            return string.Empty;
        }

        public void SetTextSize( string name, float size, bool inner = false )
        {
            GameObject component = null;
            if ( items.TryGetValue( name, out component ) )
            {
                if ( inner )
                    component.transform.Find( name ).GetComponent<TextMeshPro>().fontSize = size;
                else
                    component.GetComponent<TextMeshPro>().fontSize = size;
            }
        }

        public void SetAllColor( Color color )
        {
            foreach ( var item in items )
            {
                var sRenderer = item.Value.GetComponent<SpriteRenderer>();
                if ( sRenderer )
                    sRenderer.color = color;
            }
        }
    }
}