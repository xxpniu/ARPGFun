using Core;
using UGameTools;
using UnityEngine;

namespace BattleViews.Components
{
	//0-------2
//|	    / |
//|	  /   |
//| /	  |
//1-------3


//NormalTime + FadeTime = LifeSpan
//DelayTime + ScaleTime1 + ScaleTime2 = NormalTime
	[System.Serializable]
	public class DisplayNumerInputParam
	{
		public float RandomXInitialSpeedMin;
		public float RandomXInitialSpeedMax;

		public float RandomYInitialSpeedMin;
		public float RandomYInitialSpeedMax;

		public float RandomXaccelerationMin;
		public float RandomXaccelerationMax;

		public float RandomYaccelerationMin;
		public float RandomYaccelerationMax;

		public float NormalTime;
		public float FadeTime;

		public DisplayNumerInputParam(){}
	}


	public class GPUBillboardBuffer:XSingleton<GPUBillboardBuffer>
	{

		protected override void Awake()
		{
		
		}

		const int BC_VERTEX_EACH_BOARD = 4;
		const int BC_INDICES_EACH_BOARD = 6;
		const int BC_TEXTURE_ROW_COLUMN = 4;
		const float BC_FONT_WIDTH = 0.6f;

	
		GameObject			mGameObject;
		public Mesh				mMesh;
		Material			mMaterial;
		MeshFilter 			mFilter;
		MeshRenderer 		mRenderer;
	
		public Vector3[]			mCenters;
		public Vector4[]			mPosXYLifeScale;
		public Vector2[] 			mUv;
		public Color[]				mColors;

		public Vector2[]           mInitialSpeedXY;
		public Vector2[]           mAccelerationXY;
		public Vector3[]           mLifeSpanParam;
    
    


		Vector4[]           m_SpeedParam;       // XinitialSpeed,YinitialSpeed ,XAcceleration,YAcceleration
		Vector4[]           m_TimeParam;        // NormalTime,FadeTime,DelayTime,Maxsize,
		Vector4[]           m_TimeParamExtra;   //ScaleTime1,EndSize,ScaleTime2


		//List<Vector4>       mVec4data;
		//List<Vector4>       mVec4dataTime;
		//List<Vector4>       mVec4dataTimeExtra;
		Vector2 randomspeed;
		Vector2 randomacceleration;
		Vector3 lifeParam;

		Vector2 mUVIncrease;
	
		public uint	mMaxBoardSize = 0;
		uint	mBoardIndex = 0;
		Vector3 mPosOffset = Vector3.zero;
	
		public void OnLeaveStage()
		{
			mGameObject=null;
			mMesh=null;
			mMaterial=null;
			mFilter=null;
			mRenderer=null;
		}

		public void Init()
		{
			GPUBillboardBufferInit();
			ResourcesManager.S.LoadResourcesWithExName<Texture>("Number.png",
				(t) =>
				{
					SetTexture(t);
				});
		}

		private void GPUBillboardBufferInit()
		{
			mGameObject = new GameObject( "GPUBillboardBuffer" );
			mGameObject.transform.SetParent(this.transform, false);
			mGameObject.SetLayer("TransparentFX");
			mFilter = mGameObject.AddComponent<MeshFilter>();		
			mRenderer = mGameObject.AddComponent<MeshRenderer>();

			mRenderer.enabled = true;
			mMaterial = new Material( Shader.Find( "Billboard/BillboardParticl" ) );
			mRenderer.material = mMaterial;
		
			mMesh = new Mesh();
			mFilter.mesh = mMesh;

			//mVec4data = new List<Vector4>(4000);
			//mVec4dataTime = new List<Vector4>(4000);
			//mVec4dataTimeExtra = new List<Vector4>(4000);

			randomspeed = new Vector2();
			randomacceleration = new Vector2();
			lifeParam = new Vector3();
		}
		//-------------------------------------------------------------------------------------------//
		public void SetDisappear( float d )
		{
			mMaterial.SetFloat( "_Disappear", d );}
		//-------------------------------------------------------------------------------------------//
		public void SetTexture( Texture tex )
		{
			mMaterial.SetTexture( "_MainTex", tex );
		}
		//-------------------------------------------------------------------------------------------//
		public void SetLife(float d)
		{
			mMaterial.SetFloat( "_Life",d);
		}
		//-------------------------------------------------------------------------------------------//
		public void SetSpeed(float d)
		{
			mMaterial.SetFloat( "_Speed",d);
		}
		//-------------------------------------------------------------------------------------------//
		public void SetAcce(float d)
		{
			mMaterial.SetFloat( "_Acce",d*0.5f);
		}
		//-------------------------------------------------------------------------------------------//
		public void SetScaleTime(float d)
		{
			mMaterial.SetFloat( "_B",d*0.5f);
		}
		//-------------------------------------------------------------------------------------------//
		public void SetScaleSize(float d)
		{
			mMaterial.SetFloat( "_C",d);
		}
		//-------------------------------------------------------------------------------------------//
		public void SetPosOffset(Vector3 po)
		{
			mPosOffset = po;
		}
		//-------------------------------------------------------------------------------------------//
		public void SetScaleParams(float delaytime,
			float scaletime1,
			float scaletime2,
			float maxsize,
			float endsize)
		{
			mMaterial.SetFloat("_Delaytime", delaytime);
			mMaterial.SetFloat("_Scaletime1", scaletime1);
			mMaterial.SetFloat("_Scaletime2", scaletime2);
			mMaterial.SetFloat("_Maxsize", maxsize);
			mMaterial.SetFloat("_Endsize", endsize);
		}

		//-------------------------------------------------------------------------------------------//
		public void SetupBillboard( uint maxBoardSize )
		{
			//if( 0 == mMaxBoardSize )
			{
				mUVIncrease = new Vector2( 1.0f/BC_TEXTURE_ROW_COLUMN, 1.0f/BC_TEXTURE_ROW_COLUMN );
				mMaxBoardSize = maxBoardSize;
	
				mPosXYLifeScale = new Vector4[ maxBoardSize * BC_VERTEX_EACH_BOARD ];
				mCenters = new Vector3[ maxBoardSize * BC_VERTEX_EACH_BOARD ];

				mUv  = new Vector2[ maxBoardSize * BC_VERTEX_EACH_BOARD ];
				mInitialSpeedXY = new Vector2[maxBoardSize * BC_VERTEX_EACH_BOARD];
				mAccelerationXY = new Vector2[maxBoardSize * BC_VERTEX_EACH_BOARD];
				mLifeSpanParam = new Vector3[maxBoardSize * BC_VERTEX_EACH_BOARD];

				mColors = new Color[ maxBoardSize * BC_VERTEX_EACH_BOARD ];
	        
				mMesh.vertices = mCenters;		
				mMesh.tangents = mPosXYLifeScale;
				mMesh.colors = mColors;
				mMesh.uv  = mUv;
		
				{
					int[] Indices = new int[ maxBoardSize * BC_INDICES_EACH_BOARD ];
					for( int i = 0 ; i < maxBoardSize ; ++ i )
					{
						int index = i*BC_INDICES_EACH_BOARD;
						int vertex = i*BC_VERTEX_EACH_BOARD;
						Indices[index  ] = vertex  ;
						Indices[index+1] = vertex+1;
						Indices[index+2] = vertex+2;
					
						Indices[index+3] = vertex+2;
						Indices[index+4] = vertex+1;
						Indices[index+5] = vertex+3;
					}
					mMesh.triangles = Indices;
				}
			}
			mMesh.bounds = new Bounds( new Vector3(0,0,0), new Vector3( 100000, 100000, 100000 ) );
		}

		public void DisplayNumberRandom(string numString, Vector2 size, Vector3 center, Color clr, bool haveScale,
			DisplayNumerInputParam inpuParam)
		{

			float time = Time.timeSinceLevelLoad;
			center += mPosOffset;

			int numLength = numString.Length;

			Vector2 halfSize = new Vector2(size.x * 0.5f, size.y * 0.5f);
			float leftBio = -size.x * 0.5f * BC_FONT_WIDTH * numLength;

			int inthaveScale = 1;
			if (!haveScale)
			{
				inthaveScale = 0;
			}

			randomspeed.x = Random.Range(inpuParam.RandomXInitialSpeedMin, inpuParam.RandomXInitialSpeedMax);
			randomspeed.y = Random.Range(inpuParam.RandomYInitialSpeedMin, inpuParam.RandomYInitialSpeedMax);
			randomacceleration.x = Random.Range(inpuParam.RandomXaccelerationMin, inpuParam.RandomXaccelerationMax);
			randomacceleration.y = Random.Range(inpuParam.RandomYaccelerationMin, inpuParam.RandomYaccelerationMax);

			lifeParam.x = inpuParam.NormalTime;
			lifeParam.y = inpuParam.FadeTime;
			lifeParam.z = Random.Range(inpuParam.RandomXaccelerationMin, inpuParam.RandomXaccelerationMax);

			for (int i = 0; i < numLength; ++i)
			{
				uint indexPos = mBoardIndex * BC_VERTEX_EACH_BOARD;
				mPosXYLifeScale[indexPos].Set(-halfSize.x + leftBio + i * size.x * BC_FONT_WIDTH, halfSize.y, time,
					inthaveScale);
				mPosXYLifeScale[indexPos + 1].Set(-halfSize.x + leftBio + i * size.x * BC_FONT_WIDTH, -halfSize.y, time,
					inthaveScale);
				mPosXYLifeScale[indexPos + 2].Set(halfSize.x + leftBio + i * size.x * BC_FONT_WIDTH, halfSize.y, time,
					inthaveScale);
				mPosXYLifeScale[indexPos + 3].Set(halfSize.x + leftBio + i * size.x * BC_FONT_WIDTH, -halfSize.y, time,
					inthaveScale);

				mCenters[indexPos] = center;
				mCenters[indexPos + 1] = center;
				mCenters[indexPos + 2] = center;
				mCenters[indexPos + 3] = center;

				mColors[indexPos] = clr;
				mColors[indexPos + 1] = clr;
				mColors[indexPos + 2] = clr;
				mColors[indexPos + 3] = clr;

				mInitialSpeedXY[indexPos] = randomspeed;
				mInitialSpeedXY[indexPos + 1] = randomspeed;
				mInitialSpeedXY[indexPos + 2] = randomspeed;
				mInitialSpeedXY[indexPos + 3] = randomspeed;

				mAccelerationXY[indexPos] = randomacceleration;
				mAccelerationXY[indexPos + 1] = randomacceleration;
				mAccelerationXY[indexPos + 2] = randomacceleration;
				mAccelerationXY[indexPos + 3] = randomacceleration;

				mLifeSpanParam[indexPos] = lifeParam;
				mLifeSpanParam[indexPos + 1] = lifeParam;
				mLifeSpanParam[indexPos + 2] = lifeParam;
				mLifeSpanParam[indexPos + 3] = lifeParam;

				{
					int eachNum;
					//计算UV//
					switch (numString[i])
					{
						case '-':
						{
							eachNum = 10;
						}
							break;
						case 'M':
						{
							eachNum = 11;
						}
							break;
						case 'I':
						{
							eachNum = 12;
						}
							break;
						case 'S':
						{
							eachNum = 13;
						}
							break;
						case '+':
						{
							eachNum = 14;
						}
							break;
						case 'X':
							eachNum = 15;
							break;
						default:
						{
							eachNum = numString[i] - '0';
						}
							break;
					}

					int row = eachNum / BC_TEXTURE_ROW_COLUMN;
					int col = eachNum % BC_TEXTURE_ROW_COLUMN;
					Vector2 uvBegin = new Vector2(mUVIncrease.x * col, 1 - mUVIncrease.y * row);
					mUv[indexPos] = uvBegin;
					mUv[indexPos + 1].Set(uvBegin.x, uvBegin.y - mUVIncrease.y);
					mUv[indexPos + 2].Set(uvBegin.x + mUVIncrease.x, uvBegin.y);
					mUv[indexPos + 3].Set(uvBegin.x + mUVIncrease.x, uvBegin.y - mUVIncrease.y);
				}

				mBoardIndex = ++mBoardIndex < mMaxBoardSize ? mBoardIndex : 0;
			}


			mMesh.vertices = mCenters;
			mMesh.tangents = mPosXYLifeScale;
			mMesh.colors = mColors;
			mMesh.uv = mUv;

			mMesh.uv2 = mInitialSpeedXY;
			//mMesh.uv2 = mAccelerationXY;
			mMesh.normals = mLifeSpanParam;

		}
	}
}