﻿using UPnPNet.Models;

namespace UPnPNet
{
	public interface IUPnPServiceDescriptionXmlParser
	{
		UPnPService ParseDescription(UPnPService service, string xml);
	}
}