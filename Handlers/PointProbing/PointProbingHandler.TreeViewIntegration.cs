using devDept.Eyeshot;
using devDept.Eyeshot.Control;
using devDept.Eyeshot.Entities;
using devDept.Geometry;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace _014
{
    public partial class PointProbingHandler
    {
        public void SetTreeViewManager(TreeViewManager treeViewMgr)
        {
            treeViewManager = treeViewMgr;
            System.Diagnostics.Debug.WriteLine("✅ PointProbingHandler → TreeViewManager bağlandı");
        }
    }
}
