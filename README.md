# SerialPictureReader
串口通信数据转换灰度图像

### 串口通信
------------
摄像头采集图像后将图像灰度值通过串口发送到PC程序，数组长度满足设定的图片像素大小（`width` \* `height`），开始解析数据

### 图片显示
------------
根据算法，求出阈值
for循环遍历数组,对应像素点设置颜色
分别显示灰度化及阈值下的图像

#### Email
---------
khj1231@163.com
